using Goodtocode.Agents.Governance.Domain;
using Goodtocode.Agents.Governance.Application;
using Goodtocode.Agents.Playbook.Execution;
using Goodtocode.Agents.Playbook.Steps;

namespace Goodtocode.Agents.Playbook.Tests;

[TestClass]
public sealed class PlaybookExecutionRequestTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task DefaultRequestRunsAllActionsAndReportsThem()
    {
        var calls = new List<string>();

        var result = await new PlaybookExecutor<string, string, string, string>()
            .ExecuteAsync(new Playbook(calls), "in", PlaybookExecutionRequest<string, string>.Default, TestContext.CancellationToken);

        Assert.AreSequenceEqual(["collect", "evaluate", "record"], calls);
        Assert.AreEqual(ActionType.All, result.ActionsExecuted);
    }

    [TestMethod]
    public async Task ReplaySeedsEvidenceAndRunsRest()
    {
        var calls = new List<string>();
        var request = new PlaybookExecutionRequest<string, string> { Repeat = RepeatRequest.Create(RepeatMode.Replay, "run-1"), SeedEvidence = "seed" };

        var result = await new PlaybookExecutor<string, string, string, string>()
            .ExecuteAsync(new Playbook(calls), "in", request, TestContext.CancellationToken);

        Assert.AreSequenceEqual(["evaluate", "record"], calls);
        Assert.AreEqual("seed-finding", result.Finding);
        Assert.AreEqual(ActionType.Evaluate | ActionType.Record, result.ActionsExecuted);
        Assert.AreEqual("run-1", result.Metadata.SourceExecutionId);
    }

    [TestMethod]
    public async Task RecallIsAPureReadAndNotifiesNoRecorder()
    {
        var calls = new List<string>();
        var recorder = new CountingRecorder();
        var request = new PlaybookExecutionRequest<string, string>
        {
            Repeat = RepeatRequest.Create(RepeatMode.Recall, "run-1"), SeedEvidence = "e", SeedFinding = "f"
        };

        var result = await new PlaybookExecutor<string, string, string, string>()
            .ExecuteAsync(new Playbook(calls), "in", request, TestContext.CancellationToken, recorder);

        Assert.IsEmpty(calls);
        Assert.AreEqual(0, recorder.Count);
        Assert.AreEqual("e", result.Evidence);
        Assert.AreEqual("f", result.Finding);
        Assert.IsNull(result.Materialization);
        Assert.AreEqual(ActionType.None, result.ActionsExecuted);
    }

    [TestMethod]
    public async Task ReplayWithoutSeedThrows()
    {
        var request = new PlaybookExecutionRequest<string, string> { Repeat = RepeatRequest.Create(RepeatMode.Replay, "run-1") };

        await Assert.ThrowsAsync<InvalidOperationException>(() => new PlaybookExecutor<string, string, string, string>()
            .ExecuteAsync(new Playbook([]), "in", request, TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task RerunIgnoresSeeds()
    {
        var calls = new List<string>();
        var request = new PlaybookExecutionRequest<string, string> { SeedEvidence = "e", SeedFinding = "f" };

        await new PlaybookExecutor<string, string, string, string>()
            .ExecuteAsync(new Playbook(calls), "in", request, TestContext.CancellationToken);

        Assert.AreSequenceEqual(["collect", "evaluate", "record"], calls);
    }
    [TestMethod]
    public async Task CustomPolicyDrivesTheActions()
    {
        var calls = new List<string>();
        var policy = new DefaultRepeatPolicy(new RepeatabilityOptions { RecallRecords = true });
        var request = new PlaybookExecutionRequest<string, string>
        {
            Repeat = RepeatRequest.Create(RepeatMode.Recall, "run-1"), SeedEvidence = "e", SeedFinding = "f"
        };

        var result = await new PlaybookExecutor<string, string, string, string>(policy)
            .ExecuteAsync(new Playbook(calls), "in", request, TestContext.CancellationToken);

        Assert.AreSequenceEqual(["record"], calls);
        Assert.AreEqual(ActionType.Record, result.ActionsExecuted);
    }
    [TestMethod]
    public async Task CheckpointsAreSavedPerExecutedActionUnderRunId()
    {
        var store = new InMemoryPlaybookCheckpointStore<string, string>();
        var request = new PlaybookExecutionRequest<string, string> { RunId = "run-9" };

        await new PlaybookExecutor<string, string, string, string>()
            .ExecuteAsync(new Playbook([]), "in", request, TestContext.CancellationToken, checkpoints: store);

        var collect = await store.LoadAsync("run-9", ActionType.Collect, TestContext.CancellationToken);
        var latest = await store.LoadLatestAsync("run-9", TestContext.CancellationToken);
        Assert.AreEqual("in-evidence", collect?.Evidence);
        Assert.IsNull(collect?.Finding);
        Assert.AreEqual(ActionType.Record, latest?.ActionType);
        Assert.AreEqual("in-evidence-finding", latest?.Finding);
        Assert.IsNull(await store.LoadLatestAsync("other", TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task NoCheckpointsWithoutRunId()
    {
        var store = new InMemoryPlaybookCheckpointStore<string, string>();

        await new PlaybookExecutor<string, string, string, string>()
            .ExecuteAsync(new Playbook([]), "in", PlaybookExecutionRequest<string, string>.Default, TestContext.CancellationToken, checkpoints: store);

        Assert.IsNull(await store.LoadLatestAsync("", TestContext.CancellationToken));
    }

    private sealed class CountingRecorder : IPlaybookStepActivityRecorder<string, string, string>
    {
        public int Count { get; private set; }
        public Task OnCollectedAsync(PlaybookIdentity identity, string evidence, string? toolName, CancellationToken cancellationToken = default) { Count++; return Task.CompletedTask; }
        public Task OnEvaluatedAsync(PlaybookIdentity identity, string finding, string? toolName, CancellationToken cancellationToken = default) { Count++; return Task.CompletedTask; }
        public Task OnRecordedAsync(PlaybookIdentity identity, string materialization, string? toolName, CancellationToken cancellationToken = default) { Count++; return Task.CompletedTask; }
    }

    private sealed class Playbook(List<string> calls) : IPlaybookSteps<string, string, string, string>
    {
        public string PlaybookKey => "p";
        public string Version => "1";
        public ICollectStep<string, string> Collect => new Collector(calls);
        public IEvaluateStep<string, string> Evaluate => new Evaluator(calls);
        public IRecordStep<string, string> Record => new Recorder(calls);
    }

    private sealed class Collector(List<string> calls) : ICollectStep<string, string>
    {
        public Task<string> ExecuteAsync(string input, CancellationToken cancellationToken = default) { calls.Add("collect"); return Task.FromResult($"{input}-evidence"); }
    }

    private sealed class Evaluator(List<string> calls) : IEvaluateStep<string, string>
    {
        public Task<string> EvaluateAsync(string evidence, CancellationToken cancellationToken = default) { calls.Add("evaluate"); return Task.FromResult($"{evidence}-finding"); }
    }

    private sealed class Recorder(List<string> calls) : IRecordStep<string, string>
    {
        public Task<string> RecordAsync(string finding, CancellationToken cancellationToken = default) { calls.Add("record"); return Task.FromResult($"{finding}-materialized"); }
    }
}

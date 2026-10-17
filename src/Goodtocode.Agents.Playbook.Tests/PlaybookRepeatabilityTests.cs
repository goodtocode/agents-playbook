using Goodtocode.Agents.Governance.Application;
using Goodtocode.Agents.Governance.Domain;
using Goodtocode.Agents.Playbook.Execution;
using Goodtocode.Agents.Playbook.Steps;

namespace Goodtocode.Agents.Playbook.Tests;

/// <summary>
/// Rerun / Recall / Replay behavior of <see cref="PlaybookExecutor{TCollectInput,TEvidence,TFinding,TMaterialization}"/>
/// when driven by the Governance repeat policy.
/// </summary>
[TestClass]
public sealed class PlaybookRepeatabilityTests
{
    public TestContext TestContext { get; set; } = null!;

    private static PlaybookExecutor<string, string, string, string> Executor() => new();

    private static PlaybookExecutionRequest<string, string> Replay(string source = "run-1") => new()
    {
        Repeat = RepeatRequest.Create(RepeatMode.Replay, source),
        SeedEvidence = "seed-evidence"
    };

    private static PlaybookExecutionRequest<string, string> Recall(string source = "run-1") => new()
    {
        Repeat = RepeatRequest.Create(RepeatMode.Recall, source),
        SeedEvidence = "seed-evidence",
        SeedFinding = "seed-finding"
    };

    [TestMethod]
    public async Task ReplayEvaluatesTheSeededEvidence()
    {
        var calls = new List<string>();

        var result = await Executor().ExecuteAsync(new Steps(calls), "live-input", Replay(), TestContext.CancellationToken);

        Assert.AreEqual("seed-evidence", result.Evidence);
        Assert.AreEqual("seed-evidence-finding", result.Finding);
        Assert.AreEqual("seed-evidence-finding-materialized", result.Materialization);
        Assert.AreSequenceEqual(["evaluate", "record"], calls);
    }

    [TestMethod]
    public async Task ReplayNotifiesRecorderForExecutedActionsOnlyAndCarriesSourceAsExecutionId()
    {
        var recorder = new RecordingActivity();

        await Executor().ExecuteAsync(new Steps([]), "in", Replay("run-7"), TestContext.CancellationToken, recorder);

        Assert.AreSequenceEqual(["evaluated", "recorded"], recorder.Events);
        Assert.AreEqual("run-7", recorder.Identity?.ExecutionId);
    }

    [TestMethod]
    public async Task ReplayReportsSourceOnMetadata()
    {
        var result = await Executor().ExecuteAsync(new Steps([]), "in", Replay("run-7"), TestContext.CancellationToken);

        Assert.AreEqual("run-7", result.Metadata.SourceExecutionId);
        Assert.AreEqual(ActionType.Evaluate | ActionType.Record, result.ActionsExecuted);
    }

    [TestMethod]
    public async Task RerunReportsNoSourceAndAllActions()
    {
        var result = await Executor().ExecuteAsync(new Steps([]), "in", PlaybookExecutionRequest<string, string>.Default, TestContext.CancellationToken);

        Assert.IsNull(result.Metadata.SourceExecutionId);
        Assert.AreEqual(ActionType.All, result.ActionsExecuted);
    }

    [TestMethod]
    public async Task RecallReturnsSeedsUnchangedAndRunsNothing()
    {
        var calls = new List<string>();
        var recorder = new RecordingActivity();

        var result = await Executor().ExecuteAsync(new Steps(calls), "in", Recall(), TestContext.CancellationToken, recorder);

        Assert.IsEmpty(calls);
        Assert.IsEmpty(recorder.Events);
        Assert.AreEqual("seed-evidence", result.Evidence);
        Assert.AreEqual("seed-finding", result.Finding);
        Assert.IsNull(result.Materialization);
        Assert.AreEqual(ActionType.None, result.ActionsExecuted);
    }

    [TestMethod]
    public async Task RecallNeverSavesCheckpoints()
    {
        var store = new InMemoryPlaybookCheckpointStore<string, string>();
        var request = Recall() with { RunId = "run-9" };

        await Executor().ExecuteAsync(new Steps([]), "in", request, TestContext.CancellationToken, checkpoints: store);

        Assert.IsNull(await store.LoadLatestAsync("run-9", TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task ReplaySavesCheckpointsOnlyForActionsThatRan()
    {
        var store = new InMemoryPlaybookCheckpointStore<string, string>();
        var request = Replay() with { RunId = "run-9" };

        await Executor().ExecuteAsync(new Steps([]), "in", request, TestContext.CancellationToken, checkpoints: store);

        Assert.IsNull(await store.LoadAsync("run-9", ActionType.Collect, TestContext.CancellationToken));
        Assert.IsNotNull(await store.LoadAsync("run-9", ActionType.Evaluate, TestContext.CancellationToken));
        Assert.AreEqual(ActionType.Record, (await store.LoadLatestAsync("run-9", TestContext.CancellationToken))?.ActionType);
    }

    [TestMethod]
    public async Task RecallRecordsPolicyRunsOnlyRecordOnTheSeededFinding()
    {
        var calls = new List<string>();
        var policy = new DefaultRepeatPolicy(new RepeatabilityOptions { RecallRecords = true });

        var result = await new PlaybookExecutor<string, string, string, string>(policy)
            .ExecuteAsync(new Steps(calls), "in", Recall(), TestContext.CancellationToken);

        Assert.AreSequenceEqual(["record"], calls);
        Assert.AreEqual("seed-finding-materialized", result.Materialization);
    }

    [TestMethod]
    public async Task ContextualActionsStillReceiveContextOnReplay()
    {
        var calls = new List<string>();
        var context = new PlaybookExecutionContext(
            new PlaybookKnowledge("instruction", []), new PlaybookIdentity("p", "1"));

        var result = await Executor().ExecuteAsync(
            new ContextualSteps(calls), "in", Replay(), TestContext.CancellationToken, context: context);

        Assert.AreSequenceEqual(["contextual-evaluate"], calls.Where(c => c.StartsWith("contextual")).ToList());
        Assert.AreEqual("instruction", result.Finding);
    }

    [TestMethod]
    public async Task ReplayWithoutEvidenceSeedThrows()
    {
        var request = new PlaybookExecutionRequest<string, string> { Repeat = RepeatRequest.Create(RepeatMode.Replay, "run-1") };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Executor().ExecuteAsync(new Steps([]), "in", request, TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task RecallWithoutFindingSeedThrowsBeforeAnyWork()
    {
        var calls = new List<string>();
        var request = new PlaybookExecutionRequest<string, string>
        {
            Repeat = RepeatRequest.Create(RepeatMode.Recall, "run-1"),
            SeedEvidence = "e"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Executor().ExecuteAsync(new Steps(calls), "in", request, TestContext.CancellationToken));
        Assert.IsEmpty(calls);
    }

    [TestMethod]
    public async Task CancelledTokenThrowsBeforeAnyActionRuns()
    {
        var calls = new List<string>();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            Executor().ExecuteAsync(new Steps(calls), "in", Replay(), cts.Token));
        Assert.IsEmpty(calls);
    }

    [TestMethod]
    public void ExecutorRejectsNullPolicy()
    {
        Assert.Throws<ArgumentNullException>(() => new PlaybookExecutor<string, string, string, string>(null!));
    }

    [TestMethod]
    public async Task ExecutorRejectsNullRequest()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            Executor().ExecuteAsync(new Steps([]), "in", (PlaybookExecutionRequest<string, string>)null!, TestContext.CancellationToken));
    }

    [TestMethod]
    public void RequestDefaultsToRerun()
    {
        Assert.AreEqual(RepeatMode.Rerun, PlaybookExecutionRequest<string, string>.Default.Repeat.Mode);
    }

    private sealed class RecordingActivity : IPlaybookStepActivityRecorder<string, string, string>
    {
        public List<string> Events { get; } = [];
        public PlaybookIdentity? Identity { get; private set; }

        public Task OnCollectedAsync(PlaybookIdentity identity, string evidence, string? toolName, CancellationToken cancellationToken = default)
        {
            Identity = identity;
            Events.Add("collected");
            return Task.CompletedTask;
        }

        public Task OnEvaluatedAsync(PlaybookIdentity identity, string finding, string? toolName, CancellationToken cancellationToken = default)
        {
            Identity = identity;
            Events.Add("evaluated");
            return Task.CompletedTask;
        }

        public Task OnRecordedAsync(PlaybookIdentity identity, string materialization, string? toolName, CancellationToken cancellationToken = default)
        {
            Identity = identity;
            Events.Add("recorded");
            return Task.CompletedTask;
        }
    }

    private sealed class Steps(List<string> calls) : IPlaybookSteps<string, string, string, string>
    {
        public string PlaybookKey => "p";
        public string Version => "1";
        public ICollectStep<string, string> Collect => new CollectStep(calls);
        public IEvaluateStep<string, string> Evaluate => new EvaluateStep(calls);
        public IRecordStep<string, string> Record => new RecordStep(calls);
    }

    private sealed class ContextualSteps(List<string> calls) : IPlaybookSteps<string, string, string, string>
    {
        public string PlaybookKey => "p";
        public string Version => "1";
        public ICollectStep<string, string> Collect => new CollectStep(calls);
        public IEvaluateStep<string, string> Evaluate => new ContextualEvaluateStep(calls);
        public IRecordStep<string, string> Record => new RecordStep(calls);
    }

    private sealed class CollectStep(List<string> calls) : ICollectStep<string, string>
    {
        public Task<string> ExecuteAsync(string input, CancellationToken cancellationToken = default)
        {
            calls.Add("collect");
            return Task.FromResult($"{input}-evidence");
        }
    }

    private sealed class EvaluateStep(List<string> calls) : IEvaluateStep<string, string>
    {
        public Task<string> EvaluateAsync(string evidence, CancellationToken cancellationToken = default)
        {
            calls.Add("evaluate");
            return Task.FromResult($"{evidence}-finding");
        }
    }

    private sealed class ContextualEvaluateStep(List<string> calls) : IEvaluateStep<string, string>, IEvaluateStepContext<string, string>
    {
        public Task<string> EvaluateAsync(string evidence, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("The contextual overload must be used.");

        public Task<string> EvaluateAsync(string evidence, PlaybookExecutionContext context, CancellationToken cancellationToken = default)
        {
            calls.Add("contextual-evaluate");
            return Task.FromResult(context.Knowledge.Instruction);
        }
    }

    private sealed class RecordStep(List<string> calls) : IRecordStep<string, string>
    {
        public Task<string> RecordAsync(string finding, CancellationToken cancellationToken = default)
        {
            calls.Add("record");
            return Task.FromResult($"{finding}-materialized");
        }
    }
}

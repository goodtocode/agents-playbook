using Goodtocode.Agents.Playbook.Execution;
using Goodtocode.Agents.Playbook.Steps;

namespace Goodtocode.Agents.Playbook.Tests;

[TestClass]
public sealed class PlaybookExecutorTests
{
    [TestMethod]
    public async Task ExecuteAsync_runs_stages_in_order_and_returns_metadata()
    {
        var calls = new List<string>();
        var definition = new TestPlaybook(calls);
        var executor = new PlaybookExecutor<string, string, string, string>();

        var result = await executor.ExecuteAsync(definition, "input", TestContext.CancellationToken);

        Assert.AreSequenceEqual(["collect", "evaluate", "record"], calls);
        Assert.AreEqual("input-evidence", result.Evidence);
        Assert.AreEqual("input-evidence-finding", result.Finding);
        Assert.AreEqual("input-evidence-finding-materialized", result.Materialization);
        Assert.AreEqual("test-playbook", result.Metadata.PlaybookKey);
        Assert.AreEqual("1.0", result.Metadata.Version);
        Assert.IsGreaterThanOrEqualTo(result.Metadata.StartedUtc, result.Metadata.CompletedUtc);
    }

    [TestMethod]
    public async Task ExecuteAsync_dispatches_context_to_contextual_stages()
    {
        var calls = new List<string>();
        var definition = new ContextualTestPlaybook(calls);
        var context = new PlaybookExecutionContext(
            new PlaybookKnowledge("instruction", [new PlaybookKnowledgeItem("source", "content")]),
            new PlaybookIdentity("contextual-playbook", "2.0"));
        var executor = new PlaybookExecutor<string, string, string, string>();

        var result = await executor.ExecuteAsync(definition, "input", context, TestContext.CancellationToken);

        Assert.AreSequenceEqual(["collect", "contextual-evaluate", "contextual-record"], calls);
        Assert.AreEqual("instruction", result.Finding);
        Assert.AreEqual("contextual-playbook", result.Materialization);
    }

    [TestMethod]
    public async Task ExecuteAsync_reports_tool_names_to_activity_recorder_when_steps_are_tools()
    {
        var calls = new List<string>();
        var definition = new ToolBackedTestPlaybook(calls);
        var executor = new PlaybookExecutor<string, string, string, string>();
        var recorder = new RecordingActivityRecorder();

        var result = await executor.ExecuteAsync(definition, "input", TestContext.CancellationToken, recorder);

        Assert.AreEqual("test-playbook", recorder.Identity?.PlaybookKey);
        Assert.AreEqual("collect.tool", recorder.CollectedToolName);
        Assert.AreEqual("evaluate.tool", recorder.EvaluatedToolName);
        Assert.AreEqual("record.tool", recorder.RecordedToolName);
        Assert.AreEqual(result.Evidence, recorder.CollectedEvidence);
        Assert.AreEqual(result.Finding, recorder.EvaluatedFinding);
        Assert.AreEqual(result.Materialization, recorder.RecordedMaterialization);
    }

    [TestMethod]
    public async Task ExecuteAsync_reports_null_tool_name_when_step_is_not_a_tool()
    {
        var definition = new TestPlaybook([]);
        var executor = new PlaybookExecutor<string, string, string, string>();
        var recorder = new RecordingActivityRecorder();

        await executor.ExecuteAsync(definition, "input", TestContext.CancellationToken, recorder);

        Assert.IsNull(recorder.CollectedToolName);
        Assert.IsNull(recorder.EvaluatedToolName);
        Assert.IsNull(recorder.RecordedToolName);
    }

    [TestMethod]
    public async Task ExecuteAsync_without_activity_recorder_does_not_throw()
    {
        var definition = new TestPlaybook([]);
        var executor = new PlaybookExecutor<string, string, string, string>();

        var result = await executor.ExecuteAsync(definition, "input", TestContext.CancellationToken);

        Assert.AreEqual("input-evidence", result.Evidence);
    }

    [TestMethod]
    public async Task ExecuteAsync_honors_cancellation_before_collect()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var definition = new TestPlaybook([]);
        var executor = new PlaybookExecutor<string, string, string, string>();

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(
            () => executor.ExecuteAsync(definition, "input", cancellation.Token));
    }

    [TestMethod]
    public async Task PolicyEvaluateDefinitionBase_validates_before_evaluating()
    {
        var evaluator = new TestPolicyEvaluator();

        var result = await evaluator.EvaluateAsync("evidence", TestContext.CancellationToken);

        Assert.AreSequenceEqual(["validate", "evaluate"], evaluator.Calls);
        Assert.AreEqual("evidence-finding", result);
    }

    [TestMethod]
    public async Task ExecuteAsync_with_default_replay_context_reruns_all_stages()
    {
        var calls = new List<string>();
        var definition = new TestPlaybook(calls);
        var executor = new PlaybookExecutor<string, string, string, string>();
        var replay = new PlaybookReplayContext<string, string>();

        var result = await executor.ExecuteAsync(definition, "input", replay, TestContext.CancellationToken);

        Assert.AreSequenceEqual(["collect", "evaluate", "record"], calls);
        Assert.AreEqual("input-evidence", result.Evidence);
        Assert.AreEqual("input-evidence-finding", result.Finding);
        Assert.AreEqual(PlaybookReplayMode.Rerun, result.Metadata.ReplayMode);
        Assert.IsNull(result.Metadata.SourceExecutionId);
    }

    [TestMethod]
    public async Task ExecuteAsync_with_recall_reuses_prior_evidence_and_finding_and_only_records()
    {
        var calls = new List<string>();
        var definition = new TestPlaybook(calls);
        var executor = new PlaybookExecutor<string, string, string, string>();
        var replay = new PlaybookReplayContext<string, string>(
            PlaybookReplayMode.Recall,
            SourceExecutionId: "run-001",
            PriorEvidence: "prior-evidence",
            PriorFinding: "prior-finding");

        var result = await executor.ExecuteAsync(definition, "input", replay, TestContext.CancellationToken);

        Assert.AreSequenceEqual(["record"], calls);
        Assert.AreEqual("prior-evidence", result.Evidence);
        Assert.AreEqual("prior-finding", result.Finding);
        Assert.AreEqual("prior-finding-materialized", result.Materialization);
        Assert.AreEqual(PlaybookReplayMode.Recall, result.Metadata.ReplayMode);
        Assert.AreEqual("run-001", result.Metadata.SourceExecutionId);
    }

    [TestMethod]
    public async Task ExecuteAsync_with_replay_reuses_prior_evidence_but_reevaluates()
    {
        var calls = new List<string>();
        var definition = new TestPlaybook(calls);
        var executor = new PlaybookExecutor<string, string, string, string>();
        var replay = new PlaybookReplayContext<string, string>(
            PlaybookReplayMode.Replay,
            SourceExecutionId: "run-001",
            PriorEvidence: "prior-evidence");

        var result = await executor.ExecuteAsync(definition, "input", replay, TestContext.CancellationToken);

        Assert.AreSequenceEqual(["evaluate", "record"], calls);
        Assert.AreEqual("prior-evidence", result.Evidence);
        Assert.AreEqual("prior-evidence-finding", result.Finding);
        Assert.AreEqual(PlaybookReplayMode.Replay, result.Metadata.ReplayMode);
    }

    [TestMethod]
    public async Task ExecuteAsync_with_recall_and_missing_prior_finding_throws()
    {
        var calls = new List<string>();
        var definition = new TestPlaybook(calls);
        var executor = new PlaybookExecutor<string, string, string, string>();
        var replay = new PlaybookReplayContext<string, string>(
            PlaybookReplayMode.Recall,
            SourceExecutionId: "run-001",
            PriorEvidence: "prior-evidence");

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => executor.ExecuteAsync(definition, "input", replay, TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task ExecuteAsync_with_replay_and_missing_source_execution_id_throws()
    {
        var calls = new List<string>();
        var definition = new TestPlaybook(calls);
        var executor = new PlaybookExecutor<string, string, string, string>();
        var replay = new PlaybookReplayContext<string, string>(
            PlaybookReplayMode.Replay,
            PriorEvidence: "prior-evidence");

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => executor.ExecuteAsync(definition, "input", replay, TestContext.CancellationToken));
    }

    private sealed class TestPlaybook(List<string> calls) : IPlaybookSteps<string, string, string, string>
    {
        public string PlaybookKey => "test-playbook";
        public string Version => "1.0";
        public ICollectStep<string, string> Collect => new CollectStep(calls);
        public IEvaluateStep<string, string> Evaluate => new EvaluateStep(calls);
        public IRecordStep<string, string> Record => new RecordStep(calls);
    }

    private sealed class ContextualTestPlaybook(List<string> calls) : IPlaybookSteps<string, string, string, string>
    {
        public string PlaybookKey => "contextual-playbook";
        public string Version => "2.0";
        public ICollectStep<string, string> Collect => new CollectStep(calls);
        public IEvaluateStep<string, string> Evaluate => new ContextualEvaluateStep(calls);
        public IRecordStep<string, string> Record => new ContextualRecordStep(calls);
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

    private sealed class RecordStep(List<string> calls) : IRecordStep<string, string>
    {
        public Task<string> RecordAsync(string finding, CancellationToken cancellationToken = default)
        {
            calls.Add("record");
            return Task.FromResult($"{finding}-materialized");
        }
    }

    private sealed class ContextualEvaluateStep(List<string> calls) : IEvaluateStep<string, string>, IEvaluateStepContext<string, string>
    {
        public Task<string> EvaluateAsync(string evidence, CancellationToken cancellationToken = default)
        {
            calls.Add("evaluate");
            return Task.FromResult(evidence);
        }

        public Task<string> EvaluateAsync(string evidence, PlaybookExecutionContext context, CancellationToken cancellationToken = default)
        {
            calls.Add("contextual-evaluate");
            return Task.FromResult(context.Knowledge.Instruction);
        }
    }

    private sealed class ContextualRecordStep(List<string> calls) : IRecordStep<string, string>, IRecordStepContext<string, string>
    {
        public Task<string> RecordAsync(string finding, CancellationToken cancellationToken = default)
        {
            calls.Add("record");
            return Task.FromResult(finding);
        }

        public Task<string> RecordAsync(string finding, PlaybookExecutionContext context, CancellationToken cancellationToken = default)
        {
            calls.Add("contextual-record");
            return Task.FromResult(context.Identity.PlaybookKey);
        }
    }

    private sealed class ToolBackedTestPlaybook(List<string> calls) : IPlaybookSteps<string, string, string, string>
    {
        public string PlaybookKey => "test-playbook";
        public string Version => "1.0";
        public ICollectStep<string, string> Collect => new CollectStepTool(calls);
        public IEvaluateStep<string, string> Evaluate => new EvaluateStepTool(calls);
        public IRecordStep<string, string> Record => new RecordStepTool(calls);
    }

    private sealed class CollectStepTool(List<string> calls) : ICollectStepTool<string, string>
    {
        public string ToolName => "collect.tool";

        public Task<string> ExecuteAsync(string input, CancellationToken cancellationToken = default)
        {
            calls.Add("collect");
            return Task.FromResult($"{input}-evidence");
        }
    }

    private sealed class EvaluateStepTool(List<string> calls) : IEvaluateStepTool<string, string>
    {
        public string ToolName => "evaluate.tool";

        public Task<string> EvaluateAsync(string evidence, CancellationToken cancellationToken = default)
        {
            calls.Add("evaluate");
            return Task.FromResult($"{evidence}-finding");
        }
    }

    private sealed class RecordStepTool(List<string> calls) : IRecordStepTool<string, string>
    {
        public string ToolName => "record.tool";

        public Task<string> RecordAsync(string finding, CancellationToken cancellationToken = default)
        {
            calls.Add("record");
            return Task.FromResult($"{finding}-materialized");
        }
    }

    private sealed class RecordingActivityRecorder : IPlaybookStepActivityRecorder<string, string, string>
    {
        public PlaybookIdentity? Identity { get; private set; }
        public string? CollectedEvidence { get; private set; }
        public string? CollectedToolName { get; private set; }
        public string? EvaluatedFinding { get; private set; }
        public string? EvaluatedToolName { get; private set; }
        public string? RecordedMaterialization { get; private set; }
        public string? RecordedToolName { get; private set; }

        public Task OnCollectedAsync(PlaybookIdentity identity, string evidence, string? toolName, CancellationToken cancellationToken = default)
        {
            Identity = identity;
            CollectedEvidence = evidence;
            CollectedToolName = toolName;
            return Task.CompletedTask;
        }

        public Task OnEvaluatedAsync(PlaybookIdentity identity, string finding, string? toolName, CancellationToken cancellationToken = default)
        {
            EvaluatedFinding = finding;
            EvaluatedToolName = toolName;
            return Task.CompletedTask;
        }

        public Task OnRecordedAsync(PlaybookIdentity identity, string materialization, string? toolName, CancellationToken cancellationToken = default)
        {
            RecordedMaterialization = materialization;
            RecordedToolName = toolName;
            return Task.CompletedTask;
        }
    }

    private sealed class TestPolicyEvaluator : PolicyEvaluateDefinitionBase<string, string>
    {
        public List<string> Calls { get; } = [];

        protected override Task ValidateEvidenceAsync(string evidence, CancellationToken cancellationToken)
        {
            Calls.Add("validate");
            return Task.CompletedTask;
        }

        protected override Task<string> EvaluatePolicyAsync(string evidence, CancellationToken cancellationToken)
        {
            Calls.Add("evaluate");
            return Task.FromResult($"{evidence}-finding");
        }
    }

    public TestContext TestContext { get; set; }
}
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

        var result = await executor.ExecuteAsync(definition, "input");

        CollectionAssert.AreEqual(new[] { "collect", "evaluate", "record" }, calls);
        Assert.AreEqual("input-evidence", result.Evidence);
        Assert.AreEqual("input-evidence-finding", result.Finding);
        Assert.AreEqual("input-evidence-finding-materialized", result.Materialization);
        Assert.AreEqual("test-playbook", result.Metadata.PlaybookKey);
        Assert.AreEqual("1.0", result.Metadata.Version);
        Assert.IsTrue(result.Metadata.CompletedUtc >= result.Metadata.StartedUtc);
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

        var result = await executor.ExecuteAsync(definition, "input", context);

        CollectionAssert.AreEqual(new[] { "collect", "contextual-evaluate", "contextual-record" }, calls);
        Assert.AreEqual("instruction", result.Finding);
        Assert.AreEqual("contextual-playbook", result.Materialization);
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

        var result = await evaluator.EvaluateAsync("evidence");

        CollectionAssert.AreEqual(new[] { "validate", "evaluate" }, evaluator.Calls);
        Assert.AreEqual("evidence-finding", result);
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

    private sealed class ContextualEvaluateStep(List<string> calls) : IEvaluateStep<string, string>, IContextualEvaluateStep<string, string>
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

    private sealed class ContextualRecordStep(List<string> calls) : IRecordStep<string, string>, IContextualRecordStep<string, string>
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
}
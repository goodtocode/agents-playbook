using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.Agents.Playbook.Prompting.Tests;

public sealed record TestEvidence(string Document);

public sealed record TestFinding(bool Approved, string Summary);

public sealed record TestMaterialization(string Status);

[TestClass]
public sealed class PromptToolsTests
{
    [TestMethod]
    public async Task PromptCollectTool_deserializes_structured_response_into_typed_evidence()
    {
        var chatClient = new FakeChatClient(FakeChatClient.ToJson(new TestEvidence("Document content")));
        var tool = new PromptCollectTool<string, TestEvidence>(chatClient);
        var request = new PromptCollectRequest<string>("raw text", "Map the raw input into TestEvidence.");

        var evidence = await tool.ExecuteAsync(request, CancellationToken.None);

        Assert.AreEqual("Document content", evidence.Document);
        Assert.IsTrue(chatClient.LastPromptText!.Contains("Map the raw input into TestEvidence."));
        Assert.IsTrue(chatClient.LastPromptText!.Contains("raw text"));
    }

    [TestMethod]
    public async Task PromptEvaluateTool_renders_rubric_and_deserializes_typed_finding()
    {
        var chatClient = new FakeChatClient(FakeChatClient.ToJson(new TestFinding(true, "Looks good")));
        var tool = new PromptEvaluateTool<TestEvidence, TestFinding>(chatClient);
        var scale = new DiscreteEvaluationScale(
            "quality", [new EvaluationScaleLevel(3, "Meets standards", "Fully satisfied.")]);
        var rubric = new EvaluationRubric(
            "doc-review", "v1", [new EvaluationCriterion("clarity", "Clarity of writing")], scale);
        var request = new PromptEvaluateRequest<TestEvidence>(new TestEvidence("Document content"), rubric);

        var finding = await tool.EvaluateAsync(request, CancellationToken.None);

        Assert.IsTrue(finding.Approved);
        Assert.AreEqual("Looks good", finding.Summary);
        Assert.IsTrue(chatClient.LastPromptText!.Contains("Clarity of writing"));
        Assert.IsTrue(chatClient.LastPromptText!.Contains("Fully satisfied."));
    }

    [TestMethod]
    public async Task PromptRecordTool_deserializes_structured_response_into_typed_materialization()
    {
        var chatClient = new FakeChatClient(FakeChatClient.ToJson(new TestMaterialization("Approved")));
        var tool = new PromptRecordTool<TestFinding, TestMaterialization>(chatClient);
        var request = new PromptRecordRequest<TestFinding>(
            new TestFinding(true, "Looks good"), "Map the finding into TestMaterialization.");

        var materialization = await tool.RecordAsync(request, CancellationToken.None);

        Assert.AreEqual("Approved", materialization.Status);
        Assert.IsTrue(chatClient.LastPromptText!.Contains("Map the finding into TestMaterialization."));
    }
}

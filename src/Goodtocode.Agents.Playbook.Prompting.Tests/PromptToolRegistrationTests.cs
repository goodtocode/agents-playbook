using Goodtocode.Agents.Playbook.Steps;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.Agents.Playbook.Prompting.Tests;

[TestClass]
public sealed class PromptToolRegistrationTests
{
    [TestMethod]
    public void AddUniversalPromptTools_registers_all_three_stages_for_one_playbook_shape()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IChatClient>(new FakeChatClient(FakeChatClient.ToJson(new TestEvidence("x"))));
        services.AddUniversalPromptTools<string, TestEvidence, TestFinding, TestMaterialization>();

        using var provider = services.BuildServiceProvider();

        var collect = provider.GetRequiredKeyedService<ICollectStepTool<PromptCollectRequest<string>, TestEvidence>>(
            PromptToolKeys.UniversalCollect);
        var evaluate = provider.GetRequiredKeyedService<IEvaluateStepTool<PromptEvaluateRequest<TestEvidence>, TestFinding>>(
            PromptToolKeys.UniversalEvaluate);
        var record = provider.GetRequiredKeyedService<IRecordStepTool<PromptRecordRequest<TestFinding>, TestMaterialization>>(
            PromptToolKeys.UniversalRecord);

        Assert.IsInstanceOfType<PromptCollectTool<string, TestEvidence>>(collect);
        Assert.IsInstanceOfType<PromptEvaluateTool<TestEvidence, TestFinding>>(evaluate);
        Assert.IsInstanceOfType<PromptRecordTool<TestFinding, TestMaterialization>>(record);
    }
}

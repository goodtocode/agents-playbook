using Goodtocode.Agents.Playbook.Steps;
using Goodtocode.Agents.Playbook.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.Agents.Playbook.Tests.Tools;

[TestClass]
public sealed class PlaybookToolKeyTests
{
    [TestMethod]
    public void Create_normalizes_case_and_trims()
    {
        var key = PlaybookToolKey.Create("  Universal.Deterministic.Collect  ");

        Assert.AreEqual("universal.deterministic.collect", key.Value);
    }

    [TestMethod]
    public void Create_throws_for_invalid_characters()
    {
        Assert.ThrowsExactly<ArgumentException>(() => PlaybookToolKey.Create("bad key!"));
    }

    [TestMethod]
    public void TryCreate_returns_false_for_empty_value()
    {
        var success = PlaybookToolKey.TryCreate(string.Empty, out var result, out var error);

        Assert.IsFalse(success);
        Assert.IsTrue(result.IsEmpty);
        Assert.IsFalse(string.IsNullOrWhiteSpace(error));
    }

    [TestMethod]
    public void Empty_is_empty()
    {
        Assert.IsTrue(PlaybookToolKey.Empty.IsEmpty);
    }
}

[TestClass]
public sealed class PlaybookStepToolRegistrationTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void AddPlaybookStepTools_registers_attributed_tools_by_key()
    {
        var services = new ServiceCollection();

        services.AddPlaybookStepTools<string, string, string, string, string, string>(typeof(PlaybookStepToolRegistrationTests).Assembly);

        using var provider = services.BuildServiceProvider();
        var collect = provider.GetRequiredKeyedService<ICollectStepTool<string, string>>(PlaybookToolKey.Create("test.collect"));
        var evaluate = provider.GetRequiredKeyedService<IEvaluateStepTool<string, string>>(PlaybookToolKey.Create("test.evaluate"));
        var record = provider.GetRequiredKeyedService<IRecordStepTool<string, string>>(PlaybookToolKey.Create("test.record"));

        Assert.IsInstanceOfType<TestCollectTool>(collect);
        Assert.IsInstanceOfType<TestEvaluateTool>(evaluate);
        Assert.IsInstanceOfType<TestRecordTool>(record);
    }

    [TestMethod]
    public void AddPlaybookStepTools_throws_for_unattributed_tool()
    {
        var services = new ServiceCollection();

        Assert.ThrowsExactly<InvalidOperationException>(() =>
            services.AddPlaybookStepTools<int, int, int, int, int, int>(typeof(UnattributedCollectTool).Assembly));
    }

    [TestMethod]
    public async Task KeyedPlaybookStepToolResolver_falls_back_to_default_key_when_toolRef_is_null()
    {
        var services = new ServiceCollection();
        services.AddPlaybookStepTools<string, string, string, string, string, string>(typeof(PlaybookStepToolRegistrationTests).Assembly);
        using var provider = services.BuildServiceProvider();

        var resolver = new KeyedPlaybookStepToolResolver<string, string, string, string, string, string>(
            provider,
            PlaybookToolKey.Create("test.collect"),
            PlaybookToolKey.Create("test.evaluate"),
            PlaybookToolKey.Create("test.record"));

        var collect = resolver.ResolveCollect(null);
        var evidence = await collect.ExecuteAsync("input", TestContext.CancellationToken);

        Assert.AreEqual("input-collected", evidence);
    }
}

[PlaybookTool("test.collect")]
public sealed class TestCollectTool : ICollectStepTool<string, string>
{
    public string ToolName => "test.collect";

    public Task<string> ExecuteAsync(string input, CancellationToken cancellationToken = default) =>
        Task.FromResult($"{input}-collected");
}

[PlaybookTool("test.evaluate")]
public sealed class TestEvaluateTool : IEvaluateStepTool<string, string>
{
    public string ToolName => "test.evaluate";

    public Task<string> EvaluateAsync(string evidence, CancellationToken cancellationToken = default) =>
        Task.FromResult($"{evidence}-evaluated");
}

[PlaybookTool("test.record")]
public sealed class TestRecordTool : IRecordStepTool<string, string>
{
    public string ToolName => "test.record";

    public Task<string> RecordAsync(string finding, CancellationToken cancellationToken = default) =>
        Task.FromResult($"{finding}-recorded");
}

public sealed class UnattributedCollectTool : ICollectStepTool<int, int>
{
    public string ToolName => "unattributed";

    public Task<int> ExecuteAsync(int input, CancellationToken cancellationToken = default) =>
        Task.FromResult(input);
}


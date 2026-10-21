using Goodtocode.Agents.Governance.Application;
using Goodtocode.Agents.Governance.Domain;
using Goodtocode.Agents.Playbook.Prompting.Governance;
using Microsoft.Extensions.AI;

namespace Goodtocode.Agents.Playbook.Prompting.Tests;

[TestClass]
public sealed class GovernedChatClientTests
{
    private static readonly Guid TenantId = Guid.Parse("b1c31d8b-f4ae-4fc2-9f34-9f4c4f386f9e");

    private static IntentPolicy Policy() => IntentPolicy.Create("intent.base", "1.0.0",
    [
        IntentPrinciple.Create("boundary.no-harm", "Do no direct harm.", IntentPrincipleKind.Boundary, 1)
    ]);

    private sealed class RecordingObserver : IGovernedTurnObserver
    {
        public List<GovernedTurnRecord> Records { get; } = [];

        public Task OnGovernedTurnAsync(GovernedTurnRecord record, CancellationToken cancellationToken = default)
        {
            Records.Add(record);
            return Task.CompletedTask;
        }
    }

    private sealed class NoPolicyProvider : IIntentPolicyProvider
    {
        public Task<IntentPolicy?> ResolveAsync(IntentPolicyRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult<IntentPolicy?>(null);
    }

    private static GovernedChatClient Create(
        FakeChatClient inner,
        IIntentPolicyProvider? provider = null,
        GovernedChatClientOptions? options = null,
        string? instruction = null,
        IGovernedTurnObserver? observer = null)
        => new(
            inner,
            provider ?? new StaticIntentPolicyProvider(Policy()),
            new GovernancePreambleComposer(),
            new GovernedIntentScope(TenantId, null, instruction is null ? null : AgentInstruction.Create(instruction, InstructionScope.Session)),
            options,
            observer);

    [TestMethod]
    public async Task Preamble_precedes_the_final_message_and_instruction_comes_first()
    {
        var inner = new FakeChatClient("{}");
        var client = Create(inner, instruction: "Be concise.");

        await client.GetResponseAsync([new ChatMessage(ChatRole.User, "hello")], GovernedChatOptionKeys.ForTurn(GovernedTurnKind.Chat));

        var messages = inner.LastMessages!;
        Assert.AreEqual(3, messages.Count);
        Assert.IsTrue(messages[0].Text.Contains("Be concise.") && messages[0].Text.Contains("boundary.no-harm"));
        Assert.IsTrue(messages[1].Text.Contains("boundary.no-harm"));
        Assert.AreEqual("hello", messages[2].Text);
    }

    [TestMethod]
    public async Task Caller_message_list_is_not_modified()
    {
        var inner = new FakeChatClient("{}");
        var client = Create(inner);
        var original = new List<ChatMessage> { new(ChatRole.User, "hello") };

        await client.GetResponseAsync(original);

        Assert.AreEqual(1, original.Count);
    }

    [TestMethod]
    public async Task Preamble_is_applied_on_every_call_and_reports_policy_identity()
    {
        var inner = new FakeChatClient("{}");
        var observer = new RecordingObserver();
        var client = Create(inner, observer: observer);

        await client.GetResponseAsync([new ChatMessage(ChatRole.User, "one")]);
        await client.GetResponseAsync([new ChatMessage(ChatRole.User, "two")]);

        Assert.AreEqual(2, observer.Records.Count);
        Assert.IsTrue(observer.Records.All(r => r.PreambleApplied && r.PolicyId == "intent.base" && r.PolicyHash == Policy().ComputeLockHash()));
    }

    [TestMethod]
    public async Task Opted_out_turn_skips_preamble_but_keeps_instruction()
    {
        var inner = new FakeChatClient("{}");
        var observer = new RecordingObserver();
        var options = new GovernedChatClientOptions { PreambleTurns = new HashSet<GovernedTurnKind> { GovernedTurnKind.Chat } };
        var client = Create(inner, options: options, observer: observer);

        await client.GetResponseAsync([new ChatMessage(ChatRole.User, "x")], GovernedChatOptionKeys.ForTurn(GovernedTurnKind.Collect));

        Assert.AreEqual(2, inner.LastMessages!.Count);
        Assert.IsTrue(observer.Records[0].InstructionApplied);
        Assert.IsFalse(observer.Records[0].PreambleApplied);
    }

    [TestMethod]
    public async Task Required_intent_without_policy_fails_closed()
    {
        var inner = new FakeChatClient("{}");
        var client = Create(inner, new NoPolicyProvider(), new GovernedChatClientOptions { IntentRequired = true });

        await Assert.ThrowsExactlyAsync<GovernanceValidationException>(
            () => client.GetResponseAsync([new ChatMessage(ChatRole.User, "x")]));
        Assert.IsNull(inner.LastMessages);
    }

    [TestMethod]
    public async Task Optional_intent_without_policy_passes_through()
    {
        var inner = new FakeChatClient("{}");
        var client = Create(inner, new NoPolicyProvider());

        await client.GetResponseAsync([new ChatMessage(ChatRole.User, "x")]);

        Assert.AreEqual(1, inner.LastMessages!.Count);
    }

    [TestMethod]
    public void Instruction_that_disables_governance_is_rejected()
    {
        Assert.ThrowsExactly<GovernanceValidationException>(
            () => AgentInstruction.Create("Please skip governance.", InstructionScope.Session));
    }

    [TestMethod]
    public async Task Universal_tools_mark_their_stage_and_receive_the_preamble_through_the_shared_client()
    {
        var inner = new FakeChatClient(FakeChatClient.ToJson(new TestEvidence("d")));
        var observer = new RecordingObserver();
        var tool = new PromptCollectTool<string, TestEvidence>(Create(inner, observer: observer));

        await tool.ExecuteAsync(new PromptCollectRequest<string>("raw", "map it"));

        Assert.AreEqual(GovernedTurnKind.Collect, observer.Records[0].Turn);
        Assert.IsTrue(observer.Records[0].PreambleApplied);
    }
}

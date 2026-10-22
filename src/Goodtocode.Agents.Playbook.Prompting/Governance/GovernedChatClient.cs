using System.Runtime.CompilerServices;
using Goodtocode.Agents.Governance.Application;
using Goodtocode.Agents.Governance.Domain;
using Microsoft.Extensions.AI;

namespace Goodtocode.Agents.Playbook.Prompting.Governance;

/// <summary>
/// The single, shared enforcement seam for governed Agentic Intent. Wrapping an <see cref="IChatClient"/>
/// guarantees that, on every model call, the Instruction is applied and the Preamble is prepended for the
/// turn (Collect, Evaluate, Record or Chat), with the policy identity reported to the observer. It fails
/// closed when intent is required and no policy resolves. The Preamble is injected only into the outgoing
/// request; the caller's message list (the chat bubble sequence) is never modified.
/// </summary>
public sealed class GovernedChatClient : DelegatingChatClient
{
    private readonly IIntentPolicyProvider _policyProvider;
    private readonly IGovernancePreambleComposer _preambleComposer;
    private readonly IGovernedIntentScope _scope;
    private readonly GovernedChatClientOptions _options;
    private readonly IGovernedTurnObserver _observer;

    /// <summary>
    /// Initializes a new instance of the <see cref="GovernedChatClient"/> class.
    /// </summary>
    public GovernedChatClient(
        IChatClient innerClient,
        IIntentPolicyProvider policyProvider,
        IGovernancePreambleComposer preambleComposer,
        IGovernedIntentScope scope,
        GovernedChatClientOptions? options = null,
        IGovernedTurnObserver? observer = null)
        : base(innerClient)
    {
        _policyProvider = policyProvider ?? throw new ArgumentNullException(nameof(policyProvider));
        _preambleComposer = preambleComposer ?? throw new ArgumentNullException(nameof(preambleComposer));
        _scope = scope ?? throw new ArgumentNullException(nameof(scope));
        _options = options ?? new GovernedChatClientOptions();
        _observer = observer ?? NoOpGovernedTurnObserver.Instance;
    }

    /// <inheritdoc />
    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var governed = await GovernAsync(messages, options, cancellationToken).ConfigureAwait(false);
        return await base.GetResponseAsync(governed, options, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var governed = await GovernAsync(messages, options, cancellationToken).ConfigureAwait(false);
        await foreach (var update in base.GetStreamingResponseAsync(governed, options, cancellationToken).ConfigureAwait(false))
        {
            yield return update;
        }
    }

    private async Task<List<ChatMessage>> GovernAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(messages);

        var turn = GovernedChatOptionKeys.ReadTurn(options);
        var policy = await _policyProvider
            .ResolveAsync(new IntentPolicyRequest(_scope.TenantId, _scope.Jurisdiction), cancellationToken)
            .ConfigureAwait(false);

        if (policy is null && _options.IntentRequired)
        {
            throw new GovernanceValidationException(
            [
                new GovernanceValidationIssue(nameof(IntentRecord.PolicyId), "Agentic Intent is required but no policy resolved for the tenant and jurisdiction.")
            ]);
        }

        var governed = new List<ChatMessage>();
        var instruction = BuildInstruction(policy);
        var instructionApplied = instruction.Length > 0;
        if (instructionApplied)
        {
            governed.Add(new ChatMessage(ChatRole.System, instruction));
        }

        var original = messages.ToList();
        var preamble = policy is not null && _options.PreambleTurns.Contains(turn)
            ? _preambleComposer.Compose(new GovernancePreambleRequest { Policy = policy, Turn = turn, Mode = _options.PreambleMode }).Text
            : null;

        // The Preamble sits immediately before the final message so it precedes the message it governs.
        var preambleApplied = false;
        for (var i = 0; i < original.Count; i++)
        {
            if (preamble is not null && i == original.Count - 1)
            {
                governed.Add(new ChatMessage(ChatRole.System, preamble));
                preambleApplied = true;
            }

            governed.Add(original[i]);
        }

        await _observer.OnGovernedTurnAsync(
            new GovernedTurnRecord(turn, policy?.PolicyId, policy?.Version, policy?.ComputeLockHash(), instructionApplied, preambleApplied, _scope.Instruction?.ComputeHash()),
            cancellationToken).ConfigureAwait(false);

        return governed;
    }

    private string BuildInstruction(IntentPolicy? policy)
    {
        var parts = new List<string>();
        if (_scope.Instruction is { } instruction)
        {
            parts.Add(instruction.Text);
        }

        if (policy is not null)
        {
            parts.Add(IntentInstruction.Render(policy));
        }

        return string.Join(Environment.NewLine + Environment.NewLine, parts);
    }
}

using Goodtocode.Agents.Governance.Application;
using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.Agents.Playbook.Prompting.Governance;

/// <summary>
/// Host-supplied scope for one governed session, playbook run or instantiation: who the policy is
/// resolved for, and the Instruction that applies once for the whole scope.
/// </summary>
public interface IGovernedIntentScope
{
    /// <summary>Gets the tenant the intent policy is resolved for.</summary>
    Guid TenantId { get; }

    /// <summary>Gets the optional jurisdiction used to resolve the intent policy.</summary>
    string? Jurisdiction { get; }

    /// <summary>Gets the optional playbook-wide or session-wide Instruction.</summary>
    AgentInstruction? Instruction { get; }
}

/// <summary>
/// Simple immutable <see cref="IGovernedIntentScope"/>.
/// </summary>
public sealed record GovernedIntentScope(Guid TenantId, string? Jurisdiction = null, AgentInstruction? Instruction = null)
    : IGovernedIntentScope;

/// <summary>
/// Options for <see cref="GovernedChatClient"/>.
/// </summary>
public sealed class GovernedChatClientOptions
{
    /// <summary>
    /// Gets or sets whether Agentic Intent is required. When true and no policy resolves, every call fails closed.
    /// </summary>
    public bool IntentRequired { get; set; }

    /// <summary>Gets or sets whether the Preamble lists principle ids (Compact) or full text (Full).</summary>
    public GovernancePreambleMode PreambleMode { get; set; } = GovernancePreambleMode.Compact;

    /// <summary>
    /// Gets or sets the turns that receive a Preamble. Defaults to every turn; removing a turn is an explicit,
    /// recorded opt-out for that stage.
    /// </summary>
    public ISet<GovernedTurnKind> PreambleTurns { get; set; } = new HashSet<GovernedTurnKind>(Enum.GetValues<GovernedTurnKind>());
}

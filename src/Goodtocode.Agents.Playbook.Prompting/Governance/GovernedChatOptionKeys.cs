using Goodtocode.Agents.Governance.Domain;
using Microsoft.Extensions.AI;

namespace Goodtocode.Agents.Playbook.Prompting.Governance;

/// <summary>
/// Typed keys and helpers that tell <see cref="GovernedChatClient"/> which governed turn a call
/// belongs to (Collect, Evaluate, Record or Chat).
/// </summary>
public static class GovernedChatOptionKeys
{
    /// <summary>The <see cref="ChatOptions.AdditionalProperties"/> key holding the <see cref="GovernedTurnKind"/>.</summary>
    public const string Turn = "goodtocode.governance.turn";

    /// <summary>
    /// Creates chat options that mark the call as the given governed turn.
    /// </summary>
    /// <param name="turn">The turn kind.</param>
    /// <returns>Chat options carrying the turn.</returns>
    public static ChatOptions ForTurn(GovernedTurnKind turn) => new()
    {
        AdditionalProperties = new AdditionalPropertiesDictionary { [Turn] = turn }
    };

    internal static GovernedTurnKind ReadTurn(ChatOptions? options)
    {
        return options?.AdditionalProperties is { } properties
            && properties.TryGetValue(Turn, out var value)
            && value is GovernedTurnKind turn
                ? turn
                : GovernedTurnKind.Chat;
    }
}

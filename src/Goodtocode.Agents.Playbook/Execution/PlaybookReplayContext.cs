namespace Goodtocode.Agents.Playbook.Execution;

/// <summary>
/// Typed repeatability context for a CER execution, declaring which replay mode applies and
/// carrying the prior evidence/finding a Recall or Replay reuses.
/// </summary>
/// <typeparam name="TEvidence">The evidence type produced by Collect.</typeparam>
/// <typeparam name="TFinding">The finding type produced by Evaluate.</typeparam>
/// <param name="Mode">The declared replay mode. Defaults to <see cref="PlaybookReplayMode.Rerun"/>.</param>
/// <param name="SourceExecutionId">
/// Identifier of the prior execution this repeats. Required for <see cref="PlaybookReplayMode.Recall"/>
/// and <see cref="PlaybookReplayMode.Replay"/>.
/// </param>
/// <param name="PriorEvidence">
/// Previously collected evidence to reuse. Required for <see cref="PlaybookReplayMode.Recall"/>
/// and <see cref="PlaybookReplayMode.Replay"/>.
/// </param>
/// <param name="PriorFinding">
/// Previously evaluated finding to reuse. Required for <see cref="PlaybookReplayMode.Recall"/> only;
/// Replay re-evaluates the prior evidence instead of reusing the prior finding.
/// </param>
public sealed record PlaybookReplayContext<TEvidence, TFinding>(
    PlaybookReplayMode Mode = PlaybookReplayMode.Rerun,
    string? SourceExecutionId = null,
    TEvidence? PriorEvidence = default,
    TFinding? PriorFinding = default)
{
    /// <summary>
    /// Validates that the context carries the data required by its declared <see cref="Mode"/>.
    /// </summary>
    public void Validate()
    {
        if (Mode == PlaybookReplayMode.Rerun)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(SourceExecutionId))
        {
            throw new InvalidOperationException($"{Mode} requires a non-empty SourceExecutionId.");
        }

        if (PriorEvidence is null)
        {
            throw new InvalidOperationException($"{Mode} requires PriorEvidence.");
        }

        if (Mode == PlaybookReplayMode.Recall && PriorFinding is null)
        {
            throw new InvalidOperationException("Recall requires PriorFinding.");
        }
    }
}

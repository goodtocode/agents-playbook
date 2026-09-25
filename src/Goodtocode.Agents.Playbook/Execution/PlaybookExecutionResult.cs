namespace Goodtocode.Agents.Playbook.Execution;

/// <summary>
/// Contains the typed outputs of a completed CER execution.
/// </summary>
/// <typeparam name="TEvidence">The collected evidence type.</typeparam>
/// <typeparam name="TFinding">The evaluated finding type.</typeparam>
/// <typeparam name="TMaterialization">The recorded materialization type.</typeparam>
public sealed record PlaybookExecutionResult<TEvidence, TFinding, TMaterialization>(
    TEvidence Evidence,
    TFinding Finding,
    TMaterialization Materialization,
    PlaybookExecutionMetadata Metadata);

/// <summary>
/// Captures execution metadata without introducing a transport-specific payload.
/// </summary>
public sealed record PlaybookExecutionMetadata(
    string PlaybookKey,
    string Version,
    DateTimeOffset StartedUtc,
    DateTimeOffset CompletedUtc,
    PlaybookReplayMode ReplayMode = PlaybookReplayMode.Rerun,
    string? SourceExecutionId = null);

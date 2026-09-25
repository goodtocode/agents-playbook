namespace Goodtocode.Agents.Playbook.Steps;

/// <summary>
/// Defines a strongly typed Collect, Evaluate, and Record playbook.
/// </summary>
/// <typeparam name="TCollectInput">The input required by collection.</typeparam>
/// <typeparam name="TEvidence">The evidence produced by collection.</typeparam>
/// <typeparam name="TFinding">The finding produced by evaluation.</typeparam>
/// <typeparam name="TMaterialization">The materialization produced by recording.</typeparam>
public interface IPlaybookSteps<TCollectInput, TEvidence, TFinding, TMaterialization>
{
    /// <summary>
    /// Gets the stable playbook key.
    /// </summary>
    string PlaybookKey { get; }

    /// <summary>
    /// Gets the playbook contract version.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets the Collect stage.
    /// </summary>
    ICollectStep<TCollectInput, TEvidence> Collect { get; }

    /// <summary>
    /// Gets the Evaluate stage.
    /// </summary>
    IEvaluateStep<TEvidence, TFinding> Evaluate { get; }

    /// <summary>
    /// Gets the Record stage.
    /// </summary>
    IRecordStep<TFinding, TMaterialization> Record { get; }
}

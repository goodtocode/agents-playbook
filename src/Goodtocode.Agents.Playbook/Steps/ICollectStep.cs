namespace Goodtocode.Agents.Playbook.Steps;

/// <summary>
/// Defines a strongly typed Collect stage.
/// </summary>
/// <typeparam name="TCollectInput">The input required by collection.</typeparam>
/// <typeparam name="TEvidence">The evidence produced by collection.</typeparam>
public interface ICollectStep<in TCollectInput, TEvidence>
{
    /// <summary>
    /// Collects typed evidence.
    /// </summary>
    Task<TEvidence> ExecuteAsync(TCollectInput input, CancellationToken cancellationToken = default);
}


using System.Collections.Concurrent;
using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.Agents.Playbook.Execution;

/// <summary>
/// Typed state saved at an action boundary.
/// </summary>
/// <param name="RunId">The run identity.</param>
/// <param name="ActionType">The action just completed.</param>
/// <param name="Evidence">Evidence known at this boundary.</param>
/// <param name="Finding">Finding known at this boundary; default before Evaluate.</param>
/// <param name="SavedUtc">When the checkpoint was saved.</param>
public sealed record PlaybookCheckpoint<TEvidence, TFinding>(
    string RunId,
    ActionType ActionType,
    TEvidence? Evidence,
    TFinding? Finding,
    DateTimeOffset SavedUtc);

/// <summary>
/// Port for persisting action-boundary state, keyed by run identity. Implemented by the host (rows, EF, blob, memory).
/// </summary>
public interface IPlaybookCheckpointStore<TEvidence, TFinding>
{
    /// <summary>
    /// Saves a checkpoint, replacing any prior checkpoint for the same run and action.
    /// </summary>
    Task SaveAsync(PlaybookCheckpoint<TEvidence, TFinding> checkpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the checkpoint for a run and action, or null.
    /// </summary>
    Task<PlaybookCheckpoint<TEvidence, TFinding>?> LoadAsync(string runId, ActionType actionType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the most advanced checkpoint for a run, or null.
    /// </summary>
    Task<PlaybookCheckpoint<TEvidence, TFinding>?> LoadLatestAsync(string runId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Thread-safe in-memory reference store for tests and samples.
/// </summary>
public sealed class InMemoryPlaybookCheckpointStore<TEvidence, TFinding> : IPlaybookCheckpointStore<TEvidence, TFinding>
{
    private readonly ConcurrentDictionary<(string RunId, ActionType ActionType), PlaybookCheckpoint<TEvidence, TFinding>> _items = new();

    /// <inheritdoc />
    public Task SaveAsync(PlaybookCheckpoint<TEvidence, TFinding> checkpoint, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(checkpoint.RunId);
        _items[(checkpoint.RunId, checkpoint.ActionType)] = checkpoint;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<PlaybookCheckpoint<TEvidence, TFinding>?> LoadAsync(string runId, ActionType actionType, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.TryGetValue((runId, actionType), out var item) ? item : null);

    /// <inheritdoc />
    public Task<PlaybookCheckpoint<TEvidence, TFinding>?> LoadLatestAsync(string runId, CancellationToken cancellationToken = default)
    {
        foreach (var actionType in new[] { ActionType.Record, ActionType.Evaluate, ActionType.Collect })
        {
            if (_items.TryGetValue((runId, actionType), out var item))
            {
                return Task.FromResult<PlaybookCheckpoint<TEvidence, TFinding>?>(item);
            }
        }

        return Task.FromResult<PlaybookCheckpoint<TEvidence, TFinding>?>(null);
    }
}

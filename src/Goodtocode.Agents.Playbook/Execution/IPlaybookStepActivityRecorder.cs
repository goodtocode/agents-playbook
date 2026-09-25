namespace Goodtocode.Agents.Playbook.Execution;

/// <summary>
/// Optional observer invoked after each CER stage completes, so a host can persist or emit
/// observability/auditability evidence without the executor knowing about any storage concern.
/// </summary>
/// <typeparam name="TEvidence">The evidence produced by Collect.</typeparam>
/// <typeparam name="TFinding">The finding produced by Evaluate.</typeparam>
/// <typeparam name="TMaterialization">The materialization produced by Record.</typeparam>
public interface IPlaybookStepActivityRecorder<in TEvidence, in TFinding, in TMaterialization>
{
    /// <summary>
    /// Called after Collect produces evidence.
    /// </summary>
    Task OnCollectedAsync(
        PlaybookIdentity identity,
        TEvidence evidence,
        string? toolName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Called after Evaluate produces a finding.
    /// </summary>
    Task OnEvaluatedAsync(
        PlaybookIdentity identity,
        TFinding finding,
        string? toolName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Called after Record produces a materialization.
    /// </summary>
    Task OnRecordedAsync(
        PlaybookIdentity identity,
        TMaterialization materialization,
        string? toolName,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Default no-op activity recorder used when a host does not supply one.
/// </summary>
/// <typeparam name="TEvidence">The evidence produced by Collect.</typeparam>
/// <typeparam name="TFinding">The finding produced by Evaluate.</typeparam>
/// <typeparam name="TMaterialization">The materialization produced by Record.</typeparam>
public sealed class NoOpPlaybookStepActivityRecorder<TEvidence, TFinding, TMaterialization>
    : IPlaybookStepActivityRecorder<TEvidence, TFinding, TMaterialization>
{
    /// <summary>
    /// Gets the shared no-op instance.
    /// </summary>
    public static NoOpPlaybookStepActivityRecorder<TEvidence, TFinding, TMaterialization> Instance { get; } = new();

    /// <inheritdoc />
    public Task OnCollectedAsync(
        PlaybookIdentity identity, TEvidence evidence, string? toolName, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public Task OnEvaluatedAsync(
        PlaybookIdentity identity, TFinding finding, string? toolName, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public Task OnRecordedAsync(
        PlaybookIdentity identity, TMaterialization materialization, string? toolName, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

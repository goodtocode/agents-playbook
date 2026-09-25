using Goodtocode.Agents.Playbook.Steps;

namespace Goodtocode.Agents.Playbook.Execution;

/// <summary>
/// Executes a typed Collect, Evaluate, and Record playbook in deterministic order.
/// </summary>
/// <typeparam name="TCollectInput">The input required by collection.</typeparam>
/// <typeparam name="TEvidence">The evidence produced by collection.</typeparam>
/// <typeparam name="TFinding">The finding produced by evaluation.</typeparam>
/// <typeparam name="TMaterialization">The materialization produced by recording.</typeparam>
public sealed class PlaybookExecutor<TCollectInput, TEvidence, TFinding, TMaterialization>
{
    /// <summary>
    /// Executes all three CER stages and returns their typed results.
    /// </summary>
    public async Task<PlaybookExecutionResult<TEvidence, TFinding, TMaterialization>> ExecuteAsync(
        IPlaybookSteps<TCollectInput, TEvidence, TFinding, TMaterialization> definition,
        TCollectInput input,
        CancellationToken cancellationToken = default,
        IPlaybookStepActivityRecorder<TEvidence, TFinding, TMaterialization>? activityRecorder = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        cancellationToken.ThrowIfCancellationRequested();
        var recorder = activityRecorder ?? NoOpPlaybookStepActivityRecorder<TEvidence, TFinding, TMaterialization>.Instance;
        var identity = new PlaybookIdentity(definition.PlaybookKey, definition.Version);

        var startedUtc = DateTimeOffset.UtcNow;
        var evidence = await definition.Collect.ExecuteAsync(input, cancellationToken);
        await recorder.OnCollectedAsync(identity, evidence, ResolveToolName(definition.Collect), cancellationToken);

        var finding = await definition.Evaluate.EvaluateAsync(evidence, cancellationToken);
        await recorder.OnEvaluatedAsync(identity, finding, ResolveToolName(definition.Evaluate), cancellationToken);

        var materialization = await definition.Record.RecordAsync(finding, cancellationToken);
        await recorder.OnRecordedAsync(identity, materialization, ResolveToolName(definition.Record), cancellationToken);
        var completedUtc = DateTimeOffset.UtcNow;

        return new PlaybookExecutionResult<TEvidence, TFinding, TMaterialization>(
            evidence,
            finding,
            materialization,
            new PlaybookExecutionMetadata(
                definition.PlaybookKey,
                definition.Version,
                startedUtc,
                completedUtc));
    }

    /// <summary>
    /// Executes all three CER stages with explicit typed knowledge and governance context.
    /// Context-aware Evaluate and Record stages receive the context; legacy deterministic
    /// stages continue to execute through their original contracts.
    /// </summary>
    public async Task<PlaybookExecutionResult<TEvidence, TFinding, TMaterialization>> ExecuteAsync(
        IPlaybookSteps<TCollectInput, TEvidence, TFinding, TMaterialization> definition,
        TCollectInput input,
        PlaybookExecutionContext context,
        CancellationToken cancellationToken = default,
        IPlaybookStepActivityRecorder<TEvidence, TFinding, TMaterialization>? activityRecorder = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();
        var recorder = activityRecorder ?? NoOpPlaybookStepActivityRecorder<TEvidence, TFinding, TMaterialization>.Instance;
        var identity = new PlaybookIdentity(definition.PlaybookKey, definition.Version);

        var startedUtc = DateTimeOffset.UtcNow;
        var evidence = await definition.Collect.ExecuteAsync(input, cancellationToken);
        await recorder.OnCollectedAsync(identity, evidence, ResolveToolName(definition.Collect), cancellationToken);

        var finding = definition.Evaluate is IEvaluateStepContext<TEvidence, TFinding> contextualEvaluate
            ? await contextualEvaluate.EvaluateAsync(evidence, context, cancellationToken)
            : await definition.Evaluate.EvaluateAsync(evidence, cancellationToken);
        await recorder.OnEvaluatedAsync(identity, finding, ResolveToolName(definition.Evaluate), cancellationToken);

        var materialization = definition.Record is IRecordStepContext<TFinding, TMaterialization> contextualRecord
            ? await contextualRecord.RecordAsync(finding, context, cancellationToken)
            : await definition.Record.RecordAsync(finding, cancellationToken);
        await recorder.OnRecordedAsync(identity, materialization, ResolveToolName(definition.Record), cancellationToken);
        var completedUtc = DateTimeOffset.UtcNow;

        return new PlaybookExecutionResult<TEvidence, TFinding, TMaterialization>(
            evidence,
            finding,
            materialization,
            new PlaybookExecutionMetadata(
                definition.PlaybookKey,
                definition.Version,
                startedUtc,
                completedUtc));
    }

    /// <summary>
    /// Executes a CER playbook under an explicit repeatability replay mode. <see cref="PlaybookReplayMode.Rerun"/>
    /// (the default when <paramref name="replay"/> is omitted) runs Collect, Evaluate, and Record fresh.
    /// <see cref="PlaybookReplayMode.Recall"/> skips Collect and Evaluate and reuses
    /// <see cref="PlaybookReplayContext{TEvidence,TFinding}.PriorEvidence"/> and
    /// <see cref="PlaybookReplayContext{TEvidence,TFinding}.PriorFinding"/>, running only Record.
    /// <see cref="PlaybookReplayMode.Replay"/> skips Collect and reuses
    /// <see cref="PlaybookReplayContext{TEvidence,TFinding}.PriorEvidence"/>, but re-runs Evaluate and Record
    /// to verify exact reproduction.
    /// </summary>
    public async Task<PlaybookExecutionResult<TEvidence, TFinding, TMaterialization>> ExecuteAsync(
        IPlaybookSteps<TCollectInput, TEvidence, TFinding, TMaterialization> definition,
        TCollectInput input,
        PlaybookReplayContext<TEvidence, TFinding> replay,
        CancellationToken cancellationToken = default,
        IPlaybookStepActivityRecorder<TEvidence, TFinding, TMaterialization>? activityRecorder = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(replay);
        replay.Validate();
        cancellationToken.ThrowIfCancellationRequested();
        var recorder = activityRecorder ?? NoOpPlaybookStepActivityRecorder<TEvidence, TFinding, TMaterialization>.Instance;
        var identity = new PlaybookIdentity(definition.PlaybookKey, definition.Version, replay.SourceExecutionId);

        var startedUtc = DateTimeOffset.UtcNow;

        var evidence = replay.Mode is PlaybookReplayMode.Recall or PlaybookReplayMode.Replay
            ? replay.PriorEvidence!
            : await definition.Collect.ExecuteAsync(input, cancellationToken);
        await recorder.OnCollectedAsync(identity, evidence, ResolveToolName(definition.Collect), cancellationToken);

        var finding = replay.Mode == PlaybookReplayMode.Recall
            ? replay.PriorFinding!
            : await definition.Evaluate.EvaluateAsync(evidence, cancellationToken);
        await recorder.OnEvaluatedAsync(identity, finding, ResolveToolName(definition.Evaluate), cancellationToken);

        var materialization = await definition.Record.RecordAsync(finding, cancellationToken);
        await recorder.OnRecordedAsync(identity, materialization, ResolveToolName(definition.Record), cancellationToken);
        var completedUtc = DateTimeOffset.UtcNow;

        return new PlaybookExecutionResult<TEvidence, TFinding, TMaterialization>(
            evidence,
            finding,
            materialization,
            new PlaybookExecutionMetadata(
                definition.PlaybookKey,
                definition.Version,
                startedUtc,
                completedUtc,
                replay.Mode,
                replay.SourceExecutionId));
    }

    private static string? ResolveToolName<TIn, TOut>(ICollectStep<TIn, TOut> step) =>
        step is ICollectStepTool<TIn, TOut> tool ? tool.ToolName : null;

    private static string? ResolveToolName<TIn, TOut>(IEvaluateStep<TIn, TOut> step) =>
        step is IEvaluateStepTool<TIn, TOut> tool ? tool.ToolName : null;

    private static string? ResolveToolName<TIn, TOut>(IRecordStep<TIn, TOut> step) =>
        step is IRecordStepTool<TIn, TOut> tool ? tool.ToolName : null;
}

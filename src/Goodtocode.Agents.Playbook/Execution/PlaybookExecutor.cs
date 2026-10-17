using Goodtocode.Agents.Governance.Application;
using Goodtocode.Agents.Governance.Domain;
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
    private readonly IRepeatPolicy _repeatPolicy;

    /// <summary>
    /// Initializes an executor using the default governance repeat policy.
    /// </summary>
    public PlaybookExecutor()
        : this(new DefaultRepeatPolicy())
    {
    }

    /// <summary>
    /// Initializes an executor that plans repeats with the supplied governance policy.
    /// </summary>
    /// <param name="repeatPolicy">Decides which actions run for each repeat mode.</param>
    public PlaybookExecutor(IRepeatPolicy repeatPolicy)
    {
        _repeatPolicy = repeatPolicy ?? throw new ArgumentNullException(nameof(repeatPolicy));
    }

    /// <summary>
    /// Executes all three CER actions and returns their typed results.
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
    /// Executes all three CER actions with explicit typed knowledge and governance context.
    /// Context-aware Evaluate and Record actions receive the context; legacy deterministic
    /// actions continue to execute through their original contracts.
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
    [Obsolete("Use PlaybookExecutionRequest<TEvidence, TFinding>. Removed in the next major release.")]
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

    /// <summary>
    /// Executes a CER playbook for a governance repeat request: the policy plans which actions run, and the request's
    /// seeds stand in for skipped actions. A skipped action does not run, does not
    /// notify the activity recorder, and does not save a checkpoint; its seeded value flows to the next action.
    /// When <see cref="PlaybookExecutionRequest{TEvidence, TFinding}.RunId"/> and <paramref name="checkpoints"/> are
    /// both supplied, a checkpoint is saved after each action that runs. A skipped Record yields a default
    /// materialization; inspect <see cref="PlaybookExecutionResult{TEvidence, TFinding, TMaterialization}.ActionsExecuted"/>.
    /// </summary>
    public async Task<PlaybookExecutionResult<TEvidence, TFinding, TMaterialization>> ExecuteAsync(
        IPlaybookSteps<TCollectInput, TEvidence, TFinding, TMaterialization> definition,
        TCollectInput input,
        PlaybookExecutionRequest<TEvidence, TFinding> request,
        CancellationToken cancellationToken = default,
        IPlaybookStepActivityRecorder<TEvidence, TFinding, TMaterialization>? activityRecorder = null,
        PlaybookExecutionContext? context = null,
        IPlaybookCheckpointStore<TEvidence, TFinding>? checkpoints = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(request);
        var plan = _repeatPolicy.Plan(request.Repeat);
        request.Validate(plan);
        cancellationToken.ThrowIfCancellationRequested();
        var recorder = activityRecorder ?? NoOpPlaybookStepActivityRecorder<TEvidence, TFinding, TMaterialization>.Instance;
        var sourceExecutionId = request.Repeat.SourceExecutionId;
        var identity = new PlaybookIdentity(definition.PlaybookKey, definition.Version, sourceExecutionId);
        var store = request.RunId is null ? null : checkpoints;

        var startedUtc = DateTimeOffset.UtcNow;

        TEvidence evidence;
        if (!plan.Runs(ActionType.Collect))
        {
            evidence = request.SeedEvidence!;
        }
        else
        {
            evidence = await definition.Collect.ExecuteAsync(input, cancellationToken);
            await recorder.OnCollectedAsync(identity, evidence, ResolveToolName(definition.Collect), cancellationToken);
            await SaveAsync(store, request.RunId, ActionType.Collect, evidence, default, cancellationToken);
        }

        TFinding finding;
        if (!plan.Runs(ActionType.Evaluate))
        {
            finding = request.SeedFinding!;
        }
        else
        {
            finding = context is not null && definition.Evaluate is IEvaluateStepContext<TEvidence, TFinding> contextualEvaluate
                ? await contextualEvaluate.EvaluateAsync(evidence, context, cancellationToken)
                : await definition.Evaluate.EvaluateAsync(evidence, cancellationToken);
            await recorder.OnEvaluatedAsync(identity, finding, ResolveToolName(definition.Evaluate), cancellationToken);
            await SaveAsync(store, request.RunId, ActionType.Evaluate, evidence, finding, cancellationToken);
        }

        TMaterialization materialization = default!;
        if (plan.Runs(ActionType.Record))
        {
            materialization = context is not null && definition.Record is IRecordStepContext<TFinding, TMaterialization> contextualRecord
                ? await contextualRecord.RecordAsync(finding, context, cancellationToken)
                : await definition.Record.RecordAsync(finding, cancellationToken);
            await recorder.OnRecordedAsync(identity, materialization, ResolveToolName(definition.Record), cancellationToken);
            await SaveAsync(store, request.RunId, ActionType.Record, evidence, finding, cancellationToken);
        }

        return new PlaybookExecutionResult<TEvidence, TFinding, TMaterialization>(
            evidence,
            finding,
            materialization,
            new PlaybookExecutionMetadata(
                definition.PlaybookKey,
                definition.Version,
                startedUtc,
                DateTimeOffset.UtcNow,
                SourceExecutionId: sourceExecutionId))
        {
            ActionsExecuted = plan.Run
        };
    }

    private static Task SaveAsync(
        IPlaybookCheckpointStore<TEvidence, TFinding>? store,
        string? runId,
        ActionType actionType,
        TEvidence? evidence,
        TFinding? finding,
        CancellationToken cancellationToken) =>
        store is null
            ? Task.CompletedTask
            : store.SaveAsync(new PlaybookCheckpoint<TEvidence, TFinding>(runId!, actionType, evidence, finding, DateTimeOffset.UtcNow), cancellationToken);

    private static string? ResolveToolName<TIn, TOut>(ICollectStep<TIn, TOut> step) =>
        step is ICollectStepTool<TIn, TOut> tool ? tool.ToolName : null;

    private static string? ResolveToolName<TIn, TOut>(IEvaluateStep<TIn, TOut> step) =>
        step is IEvaluateStepTool<TIn, TOut> tool ? tool.ToolName : null;

    private static string? ResolveToolName<TIn, TOut>(IRecordStep<TIn, TOut> step) =>
        step is IRecordStepTool<TIn, TOut> tool ? tool.ToolName : null;
}

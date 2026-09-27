namespace Goodtocode.Agents.Playbook.Execution;

/// <summary>
/// Explicit, typed knowledge and identity supplied to context-aware CER stages.
/// </summary>
public sealed record PlaybookExecutionContext(
    PlaybookKnowledge Knowledge,
    PlaybookIdentity Identity,
    PlaybookGovernanceContext? Governance = null);

/// <summary>
/// Knowledge supplied to evaluation and recording.
/// </summary>
public sealed record PlaybookKnowledge(
    string Instruction,
    IReadOnlyList<PlaybookKnowledgeItem> Items,
    EvaluationRubric? Rubric = null);

/// <summary>
/// A typed knowledge item with a stable kind and source reference.
/// </summary>
public sealed record PlaybookKnowledgeItem(
    string Key,
    string Content,
    string Kind = "reference");

/// <summary>
/// Stable identity for a playbook execution.
/// </summary>
public sealed record PlaybookIdentity(
    string PlaybookKey,
    string Version,
    string? ExecutionId = null);

/// <summary>
/// Host-provided governance metadata for a playbook execution.
/// </summary>
public sealed record PlaybookGovernanceContext(
    string? PolicyVersion = null,
    string? ProfileHash = null);

/// <summary>
/// Versioned typed rubric used by an evaluation stage.
/// </summary>
public sealed record EvaluationRubric(
    string RubricId,
    string Version,
    IReadOnlyList<EvaluationCriterion> Criteria,
    IEvaluationScale Scale);

/// <summary>
/// One criterion in a typed evaluation rubric. <see cref="ScaleOverride"/> is set only when this
/// criterion's level wording differs from the rubric's shared <see cref="EvaluationRubric.Scale"/>;
/// most criteria share one scale and leave this null.
/// </summary>
public sealed record EvaluationCriterion(
    string CriterionId,
    string Description,
    double Weight = 1d,
    IEvaluationScale? ScaleOverride = null);

/// <summary>
/// Marker for the two supported evaluation scale shapes: a discrete, ordinal set of levels
/// (<see cref="DiscreteEvaluationScale"/>), or a continuous set of numeric bands
/// (<see cref="ContinuousEvaluationScale"/>). Rubric styles (holistic, analytic, checklist,
/// weighted) are compositions of these two shapes, not separate types.
/// </summary>
public interface IEvaluationScale
{
    string ScaleId { get; }
}

/// <summary>
/// A discrete, ordinal Likert-style scale shared across one or more criteria: an analytic rubric
/// uses one shared <see cref="DiscreteEvaluationScale"/> across all criteria, a holistic rubric is
/// the same shape with one criterion, and a checklist is the same shape with two levels.
/// </summary>
public sealed record DiscreteEvaluationScale(
    string ScaleId,
    IReadOnlyList<EvaluationScaleLevel> Levels) : IEvaluationScale;

/// <summary>
/// One ordinal level in a <see cref="DiscreteEvaluationScale"/>.
/// </summary>
public sealed record EvaluationScaleLevel(
    int Level,
    string Label,
    string Description);

/// <summary>
/// A continuous numeric-band scale (for example, a score of 90-100 mapped to "Excellent"), used
/// for threshold/range-based scoring rather than discrete ordinal levels.
/// </summary>
public sealed record ContinuousEvaluationScale(
    string ScaleId,
    IReadOnlyList<EvaluationScaleEntry> Entries) : IEvaluationScale;

/// <summary>
/// One named numeric band in a <see cref="ContinuousEvaluationScale"/>.
/// </summary>
public sealed record EvaluationScaleEntry(
    string Name,
    double Minimum,
    double Maximum);

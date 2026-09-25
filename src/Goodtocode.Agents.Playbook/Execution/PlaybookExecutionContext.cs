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
    EvaluationScale Scale);

/// <summary>
/// One criterion in a typed evaluation rubric.
/// </summary>
public sealed record EvaluationCriterion(
    string CriterionId,
    string Description,
    double Weight = 1d);

/// <summary>
/// Typed scale metadata for rubric evaluation.
/// </summary>
public sealed record EvaluationScale(
    string ScaleId,
    IReadOnlyList<EvaluationScaleEntry> Entries);

/// <summary>
/// One named entry in an evaluation scale.
/// </summary>
public sealed record EvaluationScaleEntry(
    string Name,
    double Minimum,
    double Maximum);

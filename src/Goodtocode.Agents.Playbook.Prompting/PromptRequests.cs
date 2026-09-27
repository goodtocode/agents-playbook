using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.Agents.Playbook.Prompting;

/// <summary>
/// Wraps a Collect stage's raw input with the full natural-language instruction describing how to
/// map it into the typed <c>TEvidence</c> output. The instruction, not the tool implementation,
/// owns 100 percent of the mapping intelligence.
/// </summary>
public sealed record PromptCollectRequest<TRawInput>(
    TRawInput RawInput,
    string MappingInstruction);

/// <summary>
/// Wraps an Evaluate stage's evidence with the rubric and any additional instruction describing
/// how to score it and map the result into the typed <c>TFinding</c> output.
/// </summary>
public sealed record PromptEvaluateRequest<TEvidence>(
    TEvidence Evidence,
    EvaluationRubric Rubric,
    string? AdditionalInstruction = null);

/// <summary>
/// Wraps a Record stage's finding with the full natural-language instruction describing how to
/// map it into the typed <c>TMaterialization</c> output.
/// </summary>
public sealed record PromptRecordRequest<TFinding>(
    TFinding Finding,
    string RecordInstruction);

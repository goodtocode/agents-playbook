namespace Goodtocode.Agents.Playbook.Materialization;

/// <summary>
/// Portable timeline event emitted by a Record stage.
/// </summary>
/// <typeparam name="TEvidenceRef">The host-defined evidence reference type.</typeparam>
public sealed record PlaybookTimelineProjection<TEvidenceRef>(
    DateTimeOffset OccurredAtUtc,
    string EventType,
    string Severity,
    string Summary,
    TEvidenceRef EvidenceRef);

/// <summary>
/// Portable narrative block emitted by a Record stage.
/// </summary>
/// <typeparam name="TEvidenceRef">The host-defined evidence reference type.</typeparam>
public sealed record PlaybookChronicleProjection<TEvidenceRef>(
    string Section,
    string Title,
    string Content,
    IReadOnlyCollection<TEvidenceRef> EvidenceRefs);

/// <summary>
/// Traceability references for a canonical projection record.
/// </summary>
/// <typeparam name="TEvidenceRef">The host-defined evidence reference type.</typeparam>
public sealed record PlaybookProjectionReferences<TEvidenceRef>(
    string ResourceRef,
    IReadOnlyCollection<TEvidenceRef> EvidenceRefs);

/// <summary>
/// Narrative facets for a canonical projection record.
/// </summary>
public sealed record PlaybookProjectionNarrative(
    string Title,
    string Narrative,
    string Interpretation,
    string Recommendation);

/// <summary>
/// Canonical projection control type kinds.
/// </summary>
public enum PlaybookControlTypeKind
{
    ValueEvaluation = 0,
    InferenceConversion = 1,
    FormatTransformation = 2
}

/// <summary>
/// Threshold/range mode for a value-evaluation payload.
/// </summary>
public enum PlaybookThresholdOrRangeMode
{
    Threshold = 0,
    Range = 1
}

/// <summary>
/// Threshold/range contract for a value-evaluation payload.
/// </summary>
public sealed record PlaybookThresholdOrRange(
    PlaybookThresholdOrRangeMode Mode,
    string ThresholdValue,
    string RangeMin,
    string RangeMax);

/// <summary>
/// Value-evaluation payload for a canonical projection record.
/// </summary>
public sealed record PlaybookValueEvaluationPayload(
    string CollectedValue,
    string ExpectedValue,
    PlaybookThresholdOrRange ThresholdOrRange,
    string EvaluationResult);

/// <summary>
/// Canonical projection record emitted by a Record stage for authority-driven mapping.
/// </summary>
/// <typeparam name="TEvidenceRef">The host-defined evidence reference type.</typeparam>
public sealed record PlaybookProjectionRecord<TEvidenceRef>(
    string ControlCode,
    string Outcome,
    string Severity,
    PlaybookProjectionNarrative Narrative,
    PlaybookProjectionReferences<TEvidenceRef> References,
    PlaybookControlTypeKind ControlType,
    PlaybookValueEvaluationPayload? ValueEvaluation,
    IReadOnlyDictionary<string, string>? Facts = null,
    IReadOnlyDictionary<string, double>? Metrics = null);

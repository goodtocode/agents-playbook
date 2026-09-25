namespace Goodtocode.Agents.Playbook.Materialization;

/// <summary>
/// Portable materialization envelope emitted by a Record stage to support host publication builders.
/// </summary>
/// <typeparam name="TEvidenceRef">The host-defined evidence reference type.</typeparam>
public record PlaybookMaterializationProjection<TEvidenceRef>(
    IReadOnlyCollection<PlaybookTimelineProjection<TEvidenceRef>> TimelineEvents,
    IReadOnlyCollection<PlaybookChronicleProjection<TEvidenceRef>> ChronicleBlocks,
    IReadOnlyCollection<TEvidenceRef> EvidenceRefs)
{
    /// <summary>
    /// Canonical authority projection records used by Chronicle and Timeline builders.
    /// </summary>
    public IReadOnlyCollection<PlaybookProjectionRecord<TEvidenceRef>> ProjectionRecords { get; init; } = [];
}

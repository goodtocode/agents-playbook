using Goodtocode.Agents.Playbook.Materialization;

namespace Goodtocode.Agents.Playbook.Tests;

[TestClass]
public sealed class PlaybookMaterializationProjectionTests
{
    [TestMethod]
    public void PlaybookCerFacts_ToDictionary_merges_dimensions_and_canonical_keys()
    {
        var facts = new PlaybookCerFacts(
            Threshold: "avg<30",
            ExpectedValue: "low pressure",
            CollectedValue: "avg=12",
            Result: "pass",
            Dimensions: new Dictionary<string, string> { ["serverName"] = "sql-01" });

        var dictionary = facts.ToDictionary();

        Assert.AreEqual("sql-01", dictionary["serverName"]);
        Assert.AreEqual("avg<30", dictionary["threshold"]);
        Assert.AreEqual("avg<30", dictionary["cerThreshold"]);
        Assert.AreEqual("pass", dictionary["cerResult"]);
    }

    [TestMethod]
    public void PlaybookMaterializationProjection_defaults_projection_records_to_empty()
    {
        var projection = new PlaybookMaterializationProjection<string>(
            TimelineEvents: [],
            ChronicleBlocks: [],
            EvidenceRefs: ["evidence-1"]);

        Assert.AreEqual(0, projection.ProjectionRecords.Count);
    }

    [TestMethod]
    public void PlaybookMaterializationProjection_is_generic_over_host_evidence_reference_type()
    {
        var evidenceRef = Guid.NewGuid();
        var projection = new PlaybookMaterializationProjection<Guid>(
            TimelineEvents: [new PlaybookTimelineProjection<Guid>(DateTimeOffset.UtcNow, "Evaluated", "High", "summary", evidenceRef)],
            ChronicleBlocks: [new PlaybookChronicleProjection<Guid>("Performance", "title", "content", [evidenceRef])],
            EvidenceRefs: [evidenceRef])
        {
            ProjectionRecords =
            [
                new PlaybookProjectionRecord<Guid>(
                    ControlCode: "control-code",
                    Outcome: "pass",
                    Severity: "High",
                    Narrative: new PlaybookProjectionNarrative("title", "narrative", "interpretation", "recommendation"),
                    References: new PlaybookProjectionReferences<Guid>(evidenceRef.ToString(), [evidenceRef]),
                    ControlType: PlaybookControlTypeKind.ValueEvaluation,
                    ValueEvaluation: new PlaybookValueEvaluationPayload(
                        "collected", "expected", new PlaybookThresholdOrRange(PlaybookThresholdOrRangeMode.Threshold, "10", string.Empty, string.Empty), "pass"))
            ]
        };

        Assert.AreEqual(evidenceRef, projection.EvidenceRefs.Single());
        Assert.AreEqual(1, projection.ProjectionRecords.Count);
    }
}

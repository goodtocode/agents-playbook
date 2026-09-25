using Goodtocode.Agents.Playbook.Application;
using Goodtocode.Agents.Playbook.Domain;

namespace Goodtocode.Agents.Playbook.Tests;

internal static class TestDataFactory
{
    public static EvaluationGovernanceRecord CreateValidGovernanceRecord()
    {
        return new EvaluationGovernanceRecord
        {
            PolicyProfileVersion = "ai-assurance.v1",
            Observability = new ObservabilityRecord
            {
                TraceId = "trace-001",
                CorrelationId = Guid.Parse("9d7201c7-33c4-4f5f-ae70-e766e10e4745"),
                EvidenceRefs =
                [
                    GovernanceReference.Parse("evidence://record/001")
                ]
            },
            Repeatability = new RepeatabilityRecord
            {
                ModelRef = "model://gpt/governed",
                ModelVersion = "1.0.0",
                PromptHash = "PROMPT-HASH-001",
                InputHash = "INPUT-HASH-001",
                DeterministicReplaySupported = true,
                Seed = 42
            },
            Auditability = new AuditabilityRecord
            {
                OwnerId = Guid.Parse("76b71ff3-f0f1-40d8-a67b-cdf64b1ca5ce"),
                TenantId = Guid.Parse("b1c31d8b-f4ae-4fc2-9f34-9f4c4f386f9e"),
                PrincipalDisplay = "owner:test",
                ToolRefs =
                [
                    GovernanceReference.Parse("tool://inference/evaluator")
                ]
            },
            Defensibility = new DefensibilityRecord
            {
                PoliciesApplied =
                [
                    GovernanceReference.Parse("policy://assurance/v1")
                ],
                JustificationRefs =
                [
                    GovernanceReference.Parse("justification://record/001")
                ],
                ReasoningSummary = "Decision is evidence-backed and policy-aligned.",
                ConfidenceScore = 0.95
            }
        };
    }

    public static GovernedEvaluationOutputSchema CreateValidOutputSchema()
    {
        return new GovernedEvaluationOutputSchema
        {
            OverallScore = 95,
            OverallLevel = "Excellent",
            OverallConfidence = 0.92,
            DefensibilitySummary = "Overall result is supported by explicit evidence and rubric links.",
            Criteria =
            [
                new GovernedEvaluationCriterionSchema
                {
                    Name = "RubricConformance",
                    Score = 95,
                    Level = "Excellent",
                    Justification = "The evidence aligns to rubric guidance.",
                    Evidence = "Evidence excerpt.",
                    RubricReference = "rubric://assurance/1.0",
                    Confidence = 0.92,
                    UncertaintyFlag = false,
                    Defensibility = "Score is consistent with rubric thresholds."
                }
            ],
            Strengths = ["Evidence traceability is complete."],
            Weaknesses = ["No material weakness observed."],
            Recommendations = ["Continue governed workflow reuse."],
            AuditTrace = new GovernedEvaluationAuditTraceSchema
            {
                ModelVersion = "1.0.0",
                RubricVersion = "rubric://assurance/1.0",
                TimestampUtc = DateTimeOffset.UtcNow,
                EvaluationId = Guid.NewGuid()
            }
        };
    }
}

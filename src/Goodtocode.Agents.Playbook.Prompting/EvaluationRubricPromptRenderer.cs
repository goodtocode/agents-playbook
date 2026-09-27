using System.Text;
using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.Agents.Playbook.Prompting;

/// <summary>
/// Deterministically renders a typed <see cref="EvaluationRubric"/> into prompt text, so the
/// rubric stays structured, versioned, and testable, while the model only ever sees rendered text.
/// </summary>
public static class EvaluationRubricPromptRenderer
{
    public static string Render(EvaluationRubric rubric)
    {
        ArgumentNullException.ThrowIfNull(rubric);

        var builder = new StringBuilder();
        builder.AppendLine($"Rubric: {rubric.RubricId} (version {rubric.Version})");

        foreach (var criterion in rubric.Criteria)
        {
            builder.AppendLine();
            builder.AppendLine($"Criterion: {criterion.CriterionId} (weight {criterion.Weight})");
            builder.AppendLine(criterion.Description);
            RenderScale(builder, criterion.ScaleOverride ?? rubric.Scale);
        }

        return builder.ToString();
    }

    private static void RenderScale(StringBuilder builder, IEvaluationScale scale)
    {
        switch (scale)
        {
            case DiscreteEvaluationScale discrete:
                foreach (var level in discrete.Levels.OrderBy(level => level.Level))
                {
                    builder.AppendLine($"  Level {level.Level} ({level.Label}): {level.Description}");
                }

                break;
            case ContinuousEvaluationScale continuous:
                foreach (var entry in continuous.Entries.OrderBy(entry => entry.Minimum))
                {
                    builder.AppendLine($"  {entry.Name}: {entry.Minimum}-{entry.Maximum}");
                }

                break;
            default:
                throw new NotSupportedException($"Unsupported evaluation scale type '{scale.GetType().Name}'.");
        }
    }
}

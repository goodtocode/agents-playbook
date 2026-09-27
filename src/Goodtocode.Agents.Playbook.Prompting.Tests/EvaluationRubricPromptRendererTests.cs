using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.Agents.Playbook.Prompting.Tests;

[TestClass]
public sealed class EvaluationRubricPromptRendererTests
{
    [TestMethod]
    public void Render_includes_discrete_scale_levels_for_each_criterion()
    {
        var scale = new DiscreteEvaluationScale(
            "quality-scale",
            [
                new EvaluationScaleLevel(0, "Not implemented", "Criterion is missing entirely."),
                new EvaluationScaleLevel(3, "Meets standards", "Criterion is fully satisfied.")
            ]);
        var rubric = new EvaluationRubric(
            "doc-review",
            "v1",
            [new EvaluationCriterion("clarity", "Clarity of writing")],
            scale);

        var rendered = EvaluationRubricPromptRenderer.Render(rubric);

        Assert.IsTrue(rendered.Contains("Clarity of writing"));
        Assert.IsTrue(rendered.Contains("Level 0 (Not implemented): Criterion is missing entirely."));
        Assert.IsTrue(rendered.Contains("Level 3 (Meets standards): Criterion is fully satisfied."));
    }

    [TestMethod]
    public void Render_uses_criterion_scale_override_instead_of_shared_scale()
    {
        var sharedScale = new DiscreteEvaluationScale(
            "shared", [new EvaluationScaleLevel(1, "Shared", "Shared level text.")]);
        var overrideScale = new DiscreteEvaluationScale(
            "override", [new EvaluationScaleLevel(1, "Override", "Override level text.")]);
        var rubric = new EvaluationRubric(
            "doc-review",
            "v1",
            [new EvaluationCriterion("clarity", "Clarity of writing", ScaleOverride: overrideScale)],
            sharedScale);

        var rendered = EvaluationRubricPromptRenderer.Render(rubric);

        Assert.IsTrue(rendered.Contains("Override level text."));
        Assert.IsFalse(rendered.Contains("Shared level text."));
    }

    [TestMethod]
    public void Render_includes_continuous_scale_bands()
    {
        var scale = new ContinuousEvaluationScale(
            "score-bands", [new EvaluationScaleEntry("Excellent", 90, 100)]);
        var rubric = new EvaluationRubric(
            "doc-review", "v1", [new EvaluationCriterion("overall", "Overall quality")], scale);

        var rendered = EvaluationRubricPromptRenderer.Render(rubric);

        Assert.IsTrue(rendered.Contains("Excellent: 90-100"));
    }
}

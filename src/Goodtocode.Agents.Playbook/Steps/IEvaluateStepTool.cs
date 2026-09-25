namespace Goodtocode.Agents.Playbook.Steps;

/// <summary>
/// A named, tool-attributed Evaluate stage. <see cref="IEvaluateStep{TEvidence,TFinding}"/> is the pure
/// workflow-stage contract (what an Evaluate stage does, deterministic or AI-agent/model-backed); this
/// contract additionally identifies *which* registered tool performs it.
/// </summary>
/// <typeparam name="TEvidence">The evidence consumed by evaluation.</typeparam>
/// <typeparam name="TFinding">The finding produced by evaluation.</typeparam>
public interface IEvaluateStepTool<in TEvidence, TFinding> : IEvaluateStep<TEvidence, TFinding>
{
    /// <summary>
    /// Gets the stable identifier of the tool performing this stage.
    /// </summary>
    string ToolName { get; }
}

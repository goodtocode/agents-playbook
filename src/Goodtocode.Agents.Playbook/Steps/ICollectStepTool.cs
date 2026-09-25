namespace Goodtocode.Agents.Playbook.Steps;

/// <summary>
/// A named, tool-attributed Collect stage. <see cref="ICollectStep{TCollectInput,TEvidence}"/> is the
/// pure workflow-stage contract (what a Collect stage does); this contract additionally identifies
/// *which* registered tool performs it, which auditability, tool-selection, and multi-tool-per-stage
/// scenarios require and a plain stage contract cannot express on its own.
/// </summary>
/// <typeparam name="TCollectInput">The input required by collection.</typeparam>
/// <typeparam name="TEvidence">The evidence produced by collection.</typeparam>
public interface ICollectStepTool<in TCollectInput, TEvidence> : ICollectStep<TCollectInput, TEvidence>
{
    /// <summary>
    /// Gets the stable identifier of the tool performing this stage.
    /// </summary>
    string ToolName { get; }
}

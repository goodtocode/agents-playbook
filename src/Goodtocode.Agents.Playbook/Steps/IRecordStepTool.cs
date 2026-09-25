namespace Goodtocode.Agents.Playbook.Steps;

/// <summary>
/// A named, tool-attributed Record stage. <see cref="IRecordStep{TFinding,TMaterialization}"/> is the
/// pure workflow-stage contract (what a Record stage does); this contract additionally identifies
/// *which* registered tool performs it. Record is expected to stay deterministic unless a future
/// implementation uses an LLM purely to reshape/enrich the artifact for storage.
/// </summary>
/// <typeparam name="TFinding">The finding consumed by recording.</typeparam>
/// <typeparam name="TMaterialization">The materialization produced for publication.</typeparam>
public interface IRecordStepTool<in TFinding, TMaterialization> : IRecordStep<TFinding, TMaterialization>
{
    /// <summary>
    /// Gets the stable identifier of the tool performing this stage.
    /// </summary>
    string ToolName { get; }
}

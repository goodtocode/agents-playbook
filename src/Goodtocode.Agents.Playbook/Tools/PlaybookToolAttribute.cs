namespace Goodtocode.Agents.Playbook.Tools;

/// <summary>
/// Declares the registration key for a Collect/Evaluate/Record tool. Applied to a concrete class
/// implementing <see cref="Steps.ICollectStepTool{TCollectInput,TEvidence}"/>,
/// <see cref="Steps.IEvaluateStepTool{TEvidence,TFinding}"/>, or
/// <see cref="Steps.IRecordStepTool{TFinding,TMaterialization}"/> so a reflection-based scanner can
/// discover and key its registration by reading the attribute alone, without constructing an
/// instance (tools typically have DI dependencies).
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PlaybookToolAttribute(string key) : Attribute
{
    /// <summary>
    /// Gets the validated registration key for the attributed tool.
    /// </summary>
    public PlaybookToolKey Key { get; } = PlaybookToolKey.Create(key);
}

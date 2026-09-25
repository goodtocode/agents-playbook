using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.Agents.Playbook.Steps;

/// <summary>
/// Opt-in Record contract for stages that apply typed execution context during materialization.
/// </summary>
/// <typeparam name="TFinding">The finding consumed by recording.</typeparam>
/// <typeparam name="TMaterialization">The materialization produced for publication.</typeparam>
public interface IContextualRecordStep<in TFinding, TMaterialization>
{
    /// <summary>
    /// Materializes a finding using explicit typed execution context.
    /// </summary>
    Task<TMaterialization> RecordAsync(
        TFinding finding,
        PlaybookExecutionContext context,
        CancellationToken cancellationToken = default);
}

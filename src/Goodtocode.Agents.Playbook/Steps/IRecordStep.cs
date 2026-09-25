namespace Goodtocode.Agents.Playbook.Steps;

/// <summary>
/// Defines a strongly typed Record stage.
/// </summary>
/// <typeparam name="TFinding">The finding consumed by recording.</typeparam>
/// <typeparam name="TMaterialization">The materialization produced for publication.</typeparam>
public interface IRecordStep<in TFinding, TMaterialization>
{
    /// <summary>
    /// Materializes a typed finding.
    /// </summary>
    Task<TMaterialization> RecordAsync(TFinding finding, CancellationToken cancellationToken = default);
}

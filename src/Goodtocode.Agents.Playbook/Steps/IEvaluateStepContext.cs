using Goodtocode.Agents.Playbook.Execution;

namespace Goodtocode.Agents.Playbook.Steps;

/// <summary>
/// Opt-in Evaluate contract for stages that consume typed playbook knowledge or rubric context.
/// </summary>
/// <typeparam name="TEvidence">The evidence consumed by evaluation.</typeparam>
/// <typeparam name="TFinding">The finding produced by evaluation.</typeparam>
public interface IEvaluateStepContext<in TEvidence, TFinding>
{
    /// <summary>
    /// Evaluates evidence using explicit typed execution context.
    /// </summary>
    Task<TFinding> EvaluateAsync(
        TEvidence evidence,
        PlaybookExecutionContext context,
        CancellationToken cancellationToken = default);
}

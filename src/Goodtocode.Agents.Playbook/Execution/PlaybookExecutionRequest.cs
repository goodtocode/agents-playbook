using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.Agents.Playbook.Execution;

/// <summary>
/// Input for a CER execution that repeats prior work. The governance <see cref="RepeatRequest"/> decides which
/// actions run; this request supplies the prior state those skipped actions stand in with.
/// </summary>
/// <typeparam name="TEvidence">The evidence type produced by Collect.</typeparam>
/// <typeparam name="TFinding">The finding type produced by Evaluate.</typeparam>
public sealed record PlaybookExecutionRequest<TEvidence, TFinding>
{
    /// <summary>Gets a request that reruns every action.</summary>
    public static PlaybookExecutionRequest<TEvidence, TFinding> Default { get; } = new();

    /// <summary>Gets the repeat mode and source execution. Defaults to Rerun.</summary>
    public RepeatRequest Repeat { get; init; } = RepeatRequest.Rerun();

    /// <summary>Gets the evidence used in place of Collect when the plan skips it.</summary>
    public TEvidence? SeedEvidence { get; init; }

    /// <summary>Gets the finding used in place of Evaluate when the plan skips it.</summary>
    public TFinding? SeedFinding { get; init; }

    /// <summary>Gets the run identity checkpoints are saved under. Checkpoints are saved only when set.</summary>
    public string? RunId { get; init; }

    /// <summary>
    /// Validates that every action the plan skips has a seed. Seeds for actions that run are ignored.
    /// </summary>
    /// <param name="plan">The governance plan for <see cref="Repeat"/>.</param>
    /// <exception cref="InvalidOperationException">A skipped action has no seed.</exception>
    public void Validate(RepeatPlan plan)
    {
        if (!plan.Runs(ActionType.Collect) && SeedEvidence is null)
        {
            throw new InvalidOperationException($"{plan.Mode} skips Collect and requires SeedEvidence.");
        }

        if (!plan.Runs(ActionType.Evaluate) && SeedFinding is null)
        {
            throw new InvalidOperationException($"{plan.Mode} skips Evaluate and requires SeedFinding.");
        }
    }
}

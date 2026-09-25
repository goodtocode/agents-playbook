namespace Goodtocode.Agents.Playbook.Steps;

/// <summary>
/// Defines a strongly typed Evaluate stage.
/// </summary>
/// <typeparam name="TEvidence">The evidence consumed by evaluation.</typeparam>
/// <typeparam name="TFinding">The finding produced by evaluation.</typeparam>
public interface IEvaluateStep<in TEvidence, TFinding>
{
    /// <summary>
    /// Evaluates typed evidence.
    /// </summary>
    Task<TFinding> EvaluateAsync(TEvidence evidence, CancellationToken cancellationToken = default);
}

/// <summary>
/// Provides a two-phase Evaluate stage that validates evidence before applying policy.
/// </summary>
/// <typeparam name="TEvidence">The evidence consumed by evaluation.</typeparam>
/// <typeparam name="TFinding">The finding produced by evaluation.</typeparam>
public abstract class PolicyEvaluateDefinitionBase<TEvidence, TFinding> : IEvaluateStep<TEvidence, TFinding>
{
    /// <inheritdoc />
    public async Task<TFinding> EvaluateAsync(TEvidence evidence, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(evidence);

        await ValidateEvidenceAsync(evidence, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        return await EvaluatePolicyAsync(evidence, cancellationToken);
    }

    /// <summary>
    /// Validates evidence before policy evaluation.
    /// </summary>
    protected abstract Task ValidateEvidenceAsync(TEvidence evidence, CancellationToken cancellationToken);

    /// <summary>
    /// Applies policy to validated evidence.
    /// </summary>
    protected abstract Task<TFinding> EvaluatePolicyAsync(TEvidence evidence, CancellationToken cancellationToken);
}

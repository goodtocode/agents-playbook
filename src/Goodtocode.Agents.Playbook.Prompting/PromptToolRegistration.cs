using Goodtocode.Agents.Playbook.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.Agents.Playbook.Prompting;

/// <summary>
/// Registers the universal, prompt-based Collect/Evaluate/Record tools for one playbook's typed
/// shape. The tool identity (<see cref="PromptCollectTool{TRawInput,TEvidence}"/> and its Evaluate/
/// Record counterparts) never changes across playbooks; only the closed evidence/finding/
/// materialization types vary, so a host calls this once per playbook shape it wants the universal
/// prompt tools available for. Unlike <c>AddPlaybookStepTools</c>'s attribute-based reflection scan
/// (built for concrete, host-authored tool types), these tools are generic over any host-defined
/// type and cannot be discovered by scanning alone.
/// </summary>
public static class PromptToolRegistration
{
    public static IServiceCollection AddUniversalPromptTools<TRawInput, TEvidence, TFinding, TMaterialization>(
        this IServiceCollection services)
    {
        services.AddKeyedScoped<
            ICollectStepTool<PromptCollectRequest<TRawInput>, TEvidence>,
            PromptCollectTool<TRawInput, TEvidence>>(PromptToolKeys.UniversalCollect);

        services.AddKeyedScoped<
            IEvaluateStepTool<PromptEvaluateRequest<TEvidence>, TFinding>,
            PromptEvaluateTool<TEvidence, TFinding>>(PromptToolKeys.UniversalEvaluate);

        services.AddKeyedScoped<
            IRecordStepTool<PromptRecordRequest<TFinding>, TMaterialization>,
            PromptRecordTool<TFinding, TMaterialization>>(PromptToolKeys.UniversalRecord);

        return services;
    }
}


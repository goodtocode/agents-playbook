using System.Reflection;
using Goodtocode.Agents.Playbook.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.Agents.Playbook.Tools;

/// <summary>
/// Discovers and registers Collect/Evaluate/Record tools by reflection: any public, non-abstract,
/// non-nested class implementing the closed <see cref="ICollectStepTool{TCollectInput,TEvidence}"/>,
/// <see cref="IEvaluateStepTool{TEvaluateInput,TFinding}"/>, or
/// <see cref="IRecordStepTool{TRecordInput,TMaterialization}"/> shape and carrying
/// <see cref="PlaybookToolAttribute"/> is keyed-registered automatically. A type that implements a
/// stage interface without the attribute fails registration explicitly rather than being silently
/// skipped or auto-keyed off a brittle string.
/// </summary>
public static class PlaybookStepToolRegistration
{
    public static IServiceCollection AddPlaybookStepTools<TCollectInput, TEvidence, TEvaluateInput, TFinding, TRecordInput, TMaterialization>(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var scanAssemblies = assemblies.Length > 0 ? assemblies : [Assembly.GetCallingAssembly()];

        RegisterStage<ICollectStepTool<TCollectInput, TEvidence>>(services, scanAssemblies);
        RegisterStage<IEvaluateStepTool<TEvaluateInput, TFinding>>(services, scanAssemblies);
        RegisterStage<IRecordStepTool<TRecordInput, TMaterialization>>(services, scanAssemblies);

        return services;
    }

    private static void RegisterStage<TStage>(IServiceCollection services, IReadOnlyCollection<Assembly> assemblies)
    {
        var stageType = typeof(TStage);
        var toolTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsAbstract: false, IsInterface: false }
                           && type.IsPublic
                           && !type.IsNested
                           && stageType.IsAssignableFrom(type))
            .Distinct()
            .ToArray();

        var seenKeys = new HashSet<PlaybookToolKey>();
        foreach (var toolType in toolTypes)
        {
            var attribute = toolType.GetCustomAttribute<PlaybookToolAttribute>()
                ?? throw new InvalidOperationException(
                    $"Tool type '{toolType.FullName}' implements '{stageType.Name}' but has no " +
                    $"[PlaybookTool(\"...\")] attribute; every registered tool must declare its key.");

            if (!seenKeys.Add(attribute.Key))
            {
                throw new InvalidOperationException(
                    $"Duplicate playbook tool key '{attribute.Key}' registered for stage '{stageType.Name}'.");
            }

            services.AddKeyedScoped(stageType, attribute.Key, toolType);
        }
    }
}

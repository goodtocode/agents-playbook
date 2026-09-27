using Goodtocode.Agents.Playbook.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.Agents.Playbook.Tools;

/// <summary>
/// Resolves Collect/Evaluate/Record tools from keyed DI registrations produced by
/// <see cref="PlaybookStepToolRegistration.AddPlaybookStepTools{TCollectInput,TEvidence,TEvaluateInput,TFinding,TRecordInput,TMaterialization}"/>,
/// falling back to the supplied default key for a stage when its contract does not name one.
/// </summary>
public sealed class KeyedPlaybookStepToolResolver<TCollectInput, TEvidence, TEvaluateInput, TFinding, TRecordInput, TMaterialization>(
    IServiceProvider serviceProvider,
    PlaybookToolKey defaultCollectKey,
    PlaybookToolKey defaultEvaluateKey,
    PlaybookToolKey defaultRecordKey)
    : IPlaybookStepToolResolver<TCollectInput, TEvidence, TEvaluateInput, TFinding, TRecordInput, TMaterialization>
{
    public ICollectStepTool<TCollectInput, TEvidence> ResolveCollect(PlaybookToolKey? toolRef) =>
        serviceProvider.GetRequiredKeyedService<ICollectStepTool<TCollectInput, TEvidence>>(toolRef ?? defaultCollectKey);

    public IEvaluateStepTool<TEvaluateInput, TFinding> ResolveEvaluate(PlaybookToolKey? toolRef) =>
        serviceProvider.GetRequiredKeyedService<IEvaluateStepTool<TEvaluateInput, TFinding>>(toolRef ?? defaultEvaluateKey);

    public IRecordStepTool<TRecordInput, TMaterialization> ResolveRecord(PlaybookToolKey? toolRef) =>
        serviceProvider.GetRequiredKeyedService<IRecordStepTool<TRecordInput, TMaterialization>>(toolRef ?? defaultRecordKey);
}

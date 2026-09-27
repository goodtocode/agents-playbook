using Goodtocode.Agents.Playbook.Steps;

namespace Goodtocode.Agents.Playbook.Tools;

/// <summary>
/// Resolves the Collect/Evaluate/Record tool a playbook's contract asks for. Each of a playbook's
/// three CER stages may name a different registered tool (or none, falling back to a host-supplied
/// default), so playbooks can mix deterministic and AI-agent/inference tools freely per stage and
/// are never locked into one tool per stage across all playbooks.
/// </summary>
/// <typeparam name="TCollectInput">The input required by the Collect stage.</typeparam>
/// <typeparam name="TEvidence">The evidence produced by the Collect stage.</typeparam>
/// <typeparam name="TEvaluateInput">The input required by the Evaluate stage.</typeparam>
/// <typeparam name="TFinding">The finding produced by the Evaluate stage.</typeparam>
/// <typeparam name="TRecordInput">The input required by the Record stage.</typeparam>
/// <typeparam name="TMaterialization">The materialization produced by the Record stage.</typeparam>
public interface IPlaybookStepToolResolver<TCollectInput, TEvidence, TEvaluateInput, TFinding, TRecordInput, TMaterialization>
{
    ICollectStepTool<TCollectInput, TEvidence> ResolveCollect(PlaybookToolKey? toolRef);

    IEvaluateStepTool<TEvaluateInput, TFinding> ResolveEvaluate(PlaybookToolKey? toolRef);

    IRecordStepTool<TRecordInput, TMaterialization> ResolveRecord(PlaybookToolKey? toolRef);
}

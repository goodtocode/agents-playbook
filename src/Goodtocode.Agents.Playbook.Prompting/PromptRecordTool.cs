using Goodtocode.Agents.Playbook.Steps;
using Microsoft.Extensions.AI;

namespace Goodtocode.Agents.Playbook.Prompting;

/// <summary>
/// Universal, prompt-based Record tool: 100 percent of the mapping intelligence lives in
/// <see cref="PromptRecordRequest{TFinding}.RecordInstruction"/>, not in this class.
/// </summary>
public sealed class PromptRecordTool<TFinding, TMaterialization>(IChatClient chatClient)
    : IRecordStepTool<PromptRecordRequest<TFinding>, TMaterialization>
{
    public string ToolName => PromptToolKeys.UniversalRecord.Value;

    public async Task<TMaterialization> RecordAsync(PromptRecordRequest<TFinding> finding, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(finding);

        var prompt =
            $"""
            {finding.RecordInstruction}

            Finding to record:
            {finding.Finding}
            """;

        var response = await chatClient.GetResponseAsync<TMaterialization>(prompt, cancellationToken: cancellationToken);
        return response.Result;
    }
}

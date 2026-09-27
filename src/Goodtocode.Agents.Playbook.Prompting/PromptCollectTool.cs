using Goodtocode.Agents.Playbook.Steps;
using Microsoft.Extensions.AI;

namespace Goodtocode.Agents.Playbook.Prompting;

/// <summary>
/// Universal, prompt-based Collect tool: 100 percent of the mapping intelligence lives in
/// <see cref="PromptCollectRequest{TRawInput}.MappingInstruction"/>, not in this class. One
/// instance is reusable across every playbook, regardless of its typed evidence shape.
/// </summary>
public sealed class PromptCollectTool<TRawInput, TEvidence>(IChatClient chatClient)
    : ICollectStepTool<PromptCollectRequest<TRawInput>, TEvidence>
{
    public string ToolName => PromptToolKeys.UniversalCollect.Value;

    public async Task<TEvidence> ExecuteAsync(PromptCollectRequest<TRawInput> input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var prompt =
            $"""
            {input.MappingInstruction}

            Raw input:
            {input.RawInput}
            """;

        var response = await chatClient.GetResponseAsync<TEvidence>(prompt, cancellationToken: cancellationToken);
        return response.Result;
    }
}

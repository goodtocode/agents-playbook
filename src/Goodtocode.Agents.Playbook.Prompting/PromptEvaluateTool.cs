using Goodtocode.Agents.Playbook.Steps;
using Microsoft.Extensions.AI;

namespace Goodtocode.Agents.Playbook.Prompting;

/// <summary>
/// Universal, prompt-based Evaluate tool: the rubric is rendered deterministically via
/// <see cref="EvaluationRubricPromptRenderer"/> and combined with any additional instruction, so
/// 100 percent of the scoring criteria and output-mapping intelligence lives in the rubric and
/// instruction, not in this class.
/// </summary>
public sealed class PromptEvaluateTool<TEvidence, TFinding>(IChatClient chatClient)
    : IEvaluateStepTool<PromptEvaluateRequest<TEvidence>, TFinding>
{
    public string ToolName => PromptToolKeys.UniversalEvaluate.Value;

    public async Task<TFinding> EvaluateAsync(PromptEvaluateRequest<TEvidence> evidence, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evidence);

        var renderedRubric = EvaluationRubricPromptRenderer.Render(evidence.Rubric);
        var prompt =
            $"""
            {evidence.AdditionalInstruction}

            {renderedRubric}

            Evidence to evaluate:
            {evidence.Evidence}
            """;

        var response = await chatClient.GetResponseAsync<TFinding>(prompt, cancellationToken: cancellationToken);
        return response.Result;
    }
}

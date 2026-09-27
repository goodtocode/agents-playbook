using Goodtocode.Agents.Playbook.Tools;

namespace Goodtocode.Agents.Playbook.Prompting;

/// <summary>
/// Well-known keys for the universal, prompt-based CER tools every playbook can register against,
/// mirroring the naming convention a host uses for its own universal deterministic tools.
/// </summary>
public static class PromptToolKeys
{
    public static readonly PlaybookToolKey UniversalCollect = PlaybookToolKey.Create("universal.prompt.collect");

    public static readonly PlaybookToolKey UniversalEvaluate = PlaybookToolKey.Create("universal.prompt.evaluate");

    public static readonly PlaybookToolKey UniversalRecord = PlaybookToolKey.Create("universal.prompt.record");
}

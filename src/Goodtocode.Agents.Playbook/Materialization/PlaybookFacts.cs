namespace Goodtocode.Agents.Playbook.Materialization;

/// <summary>
/// Portable deterministic query declaration used by a Collect stage.
/// </summary>
public sealed record PlaybookQueryDefinition(string QueryKey, string CommandText, string Purpose);

/// <summary>
/// Deterministic CER fact payload for controls that evaluate observed state against expected state and threshold.
/// </summary>
public sealed record PlaybookFacts(
    string Threshold,
    string ExpectedValue,
    string CollectedValue,
    string Result,
    IReadOnlyDictionary<string, string>? Dimensions = null)
{
    /// <summary>
    /// Converts CER facts to canonical projection-record dictionary keys.
    /// </summary>
    public IReadOnlyDictionary<string, string> ToDictionary()
    {
        var facts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (Dimensions is not null)
        {
            foreach (var dimension in Dimensions)
            {
                facts[dimension.Key] = dimension.Value;
            }
        }

        facts["threshold"] = Threshold;
        facts["expectedValue"] = ExpectedValue;
        facts["evaluatedValue"] = ExpectedValue;
        facts["collectedValue"] = CollectedValue;
        facts["result"] = Result;
        facts["scale"] = Threshold;
        facts["cerThreshold"] = Threshold;
        facts["cerExpectedValue"] = ExpectedValue;
        facts["cerCollectedValue"] = CollectedValue;
        facts["cerResult"] = Result;

        return facts;
    }
}

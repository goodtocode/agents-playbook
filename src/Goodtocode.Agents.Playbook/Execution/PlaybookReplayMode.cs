namespace Goodtocode.Agents.Playbook.Execution;

/// <summary>
/// Declares which of the three repeatability behaviors a CER execution is performing.
/// </summary>
public enum PlaybookReplayMode
{
    /// <summary>
    /// Fresh execution: run Collect, Evaluate, and Record. A first-time execution is always a
    /// Rerun; this is the default when no replay context is supplied.
    /// </summary>
    Rerun = 0,

    /// <summary>
    /// Rehydrate a prior execution's evidence and finding without running Collect or Evaluate.
    /// Record still runs so materialization can be re-rendered from the recalled finding.
    /// </summary>
    Recall = 1,

    /// <summary>
    /// Reuse prior evidence (skip Collect) and re-run Evaluate and Record against it, to verify
    /// exact reproduction of a governed result.
    /// </summary>
    Replay = 2
}

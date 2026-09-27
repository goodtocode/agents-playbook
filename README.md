# Goodtocode.Agents.Playbook

Strongly typed Collect, Evaluate, and Record (CER) playbook contracts with a deterministic execution engine.


[![NuGet CI/CD](https://github.com/Goodtocode/agents-playbook/actions/workflows/agents-playbook-nuget.yml/badge.svg)](https://github.com/Goodtocode/agents-playbook/actions/workflows/agents-playbook-nuget.yml)
[![NuGet](https://img.shields.io/nuget/v/Goodtocode.Agents.Playbook.svg)](https://www.nuget.org/packages/Goodtocode.Agents.Playbook)

Strongly typed **Collect, Evaluate, and Record (CER)** playbook contracts with deterministic execution for .NET applications and AI-agent workflows.

Goodtocode.Agents.Playbook gives a host a small, explicit execution boundary:

1. **Collect** converts typed input into typed evidence.
2. **Evaluate** converts evidence into a typed finding.
3. **Record** converts the finding into a typed materialization.

The executor owns ordering, cancellation, optional execution context, and execution metadata. Your application owns the actual collection, policy, model calls, persistence, and transport.

## Why Playbook?

AI workflows often grow into a collection of framework-specific helpers: one package for prompt orchestration, another for evaluation, another for tracing, and another for persistence. That can make the business flow difficult to discover and the contracts difficult to test.

Playbook provides one small, framework-independent contract for the part that should remain stable across those choices:

- **Typed boundaries:** input, evidence, finding, and materialization are generic types rather than loosely shaped dictionaries.
- **Deterministic orchestration:** CER stages run in a fixed order and receive the same cancellation signal.
- **Explicit context:** knowledge, identity, rubric, and host governance metadata are passed deliberately instead of hidden in ambient state.
- **Incremental adoption:** existing stage implementations use the basic interfaces; context-aware Evaluate and Record stages are opt-in.
- **Testable by design:** stage behavior can be tested without an agent framework, model provider, database, or web host.
- **Small dependency surface:** the core package targets .NET 10 and does not require Microsoft Agent Framework, Semantic Kernel, a model SDK, or a persistence provider.

This is not a replacement for an agent framework. It is the typed playbook boundary that can sit underneath one.

## Install

```powershell
dotnet add package Goodtocode.Agents.Playbook
```

The package is published to NuGet. Pin a version in production and commit your dependency lock information according to your repository policy:

```powershell
dotnet add package Goodtocode.Agents.Playbook --version <version>
```

## Quick Start

Define the three stages and compose them into a playbook:

```csharp
using Goodtocode.Agents.Playbook.Execution;
using Goodtocode.Agents.Playbook.Steps;

public sealed record ReviewRequest(string Document);
public sealed record ReviewEvidence(string Document, IReadOnlyList<string> Sources);
public sealed record ReviewFinding(bool Approved, string Summary);
public sealed record ReviewRecord(string Status, string Summary);

public sealed class CollectReviewEvidence : ICollectStep<ReviewRequest, ReviewEvidence>
{
	public Task<ReviewEvidence> ExecuteAsync(
		ReviewRequest input,
		CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return Task.FromResult(new ReviewEvidence(
			input.Document,
			["policy://review/v1"]));
	}
}

public sealed class EvaluateReview : IEvaluateStep<ReviewEvidence, ReviewFinding>
{
	public Task<ReviewFinding> EvaluateAsync(
		ReviewEvidence evidence,
		CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		var approved = !string.IsNullOrWhiteSpace(evidence.Document);
		return Task.FromResult(new ReviewFinding(
			approved,
			approved ? "Document is ready for review." : "Document is empty."));
	}
}

public sealed class RecordReview : IRecordStep<ReviewFinding, ReviewRecord>
{
	public Task<ReviewRecord> RecordAsync(
		ReviewFinding finding,
		CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		return Task.FromResult(new ReviewRecord(
			finding.Approved ? "approved" : "rejected",
			finding.Summary));
	}
}

public sealed class DocumentReviewPlaybook
	: IPlaybookSteps<ReviewRequest, ReviewEvidence, ReviewFinding, ReviewRecord>
{
	public string PlaybookKey => "document-review";
	public string Version => "1.0";
	public ICollectStep<ReviewRequest, ReviewEvidence> Collect { get; } = new CollectReviewEvidence();
	public IEvaluateStep<ReviewEvidence, ReviewFinding> Evaluate { get; } = new EvaluateReview();
	public IRecordStep<ReviewFinding, ReviewRecord> Record { get; } = new RecordReview();
}
```

Execute it from an application, service, worker, or agent adapter:

```csharp
var executor = new PlaybookExecutor<ReviewRequest, ReviewEvidence, ReviewFinding, ReviewRecord>();
var result = await executor.ExecuteAsync(
	new DocumentReviewPlaybook(),
	new ReviewRequest("Document content"),
	cancellationToken);

Console.WriteLine(result.Materialization.Status);
Console.WriteLine(result.Metadata.PlaybookKey);
Console.WriteLine(result.Metadata.CompletedUtc);
```

`result` contains the typed `Evidence`, `Finding`, and `Materialization`, plus `PlaybookExecutionMetadata` with the playbook key, version, start time, and completion time.

## Explicit Execution Context

Evaluate and Record stages can opt into typed knowledge and identity without changing the basic CER contract:

```csharp
var context = new PlaybookExecutionContext(
	new PlaybookKnowledge(
		"Use the approved review rubric.",
		[new PlaybookKnowledgeItem("policy", "policy://review/v1")]),
	new PlaybookIdentity(
		"document-review",
		"1.0",
		ExecutionId: "run-123"),
	new PlaybookGovernanceContext(PolicyVersion: "review-policy-1"));

var result = await executor.ExecuteAsync(
	new DocumentReviewPlaybook(),
	new ReviewRequest("Document content"),
	context,
	cancellationToken);
```

Context-aware stages implement `IContextualEvaluateStep<,>` or `IContextualRecordStep<,>`. The executor dispatches to those contracts when present and preserves the ordinary stage contract otherwise.

For policy stages that should validate evidence before applying policy, derive from `PolicyEvaluateDefinitionBase<TEvidence, TFinding>` and implement `ValidateEvidenceAsync` and `EvaluatePolicyAsync`.

## Rubric Scales: Discrete and Continuous

`EvaluationRubric` (used by `PlaybookKnowledge.Rubric`) models a rubric's scale as one of exactly two shapes, not one class per rubric style:

- `DiscreteEvaluationScale` — an ordered, ordinal Likert-style scale (`EvaluationScaleLevel(Level, Label, Description)`), shared across one or more criteria. A holistic rubric, an analytic rubric, and a checklist are all this same shape — the difference is only how many criteria and levels you supply, not a different type.
- `ContinuousEvaluationScale` — numeric bands (`EvaluationScaleEntry(Name, Minimum, Maximum)`), for threshold/range-based scoring (for example, 90-100 = "Excellent") rather than discrete ordinal levels.

Both implement `IEvaluationScale`, so `EvaluationRubric.Scale` and `EvaluationCriterion.ScaleOverride` are polymorphic over either shape.

```csharp
var scale = new DiscreteEvaluationScale(
	"quality-scale",
	[
		new EvaluationScaleLevel(0, "Not implemented", "Criterion is missing entirely."),
		new EvaluationScaleLevel(1, "Unsatisfactory", "Criterion is attempted but falls well short."),
		new EvaluationScaleLevel(2, "Working towards satisfactory", "Criterion is partially met."),
		new EvaluationScaleLevel(3, "Meets standards", "Criterion is fully satisfied."),
		new EvaluationScaleLevel(4, "Exceeds standards", "Criterion is satisfied with notable strength."),
		new EvaluationScaleLevel(5, "Greatly exceeds standards", "Criterion is satisfied exceptionally.")
	]);

var rubric = new EvaluationRubric(
	"document-review-v1",
	"1.0",
	[
		new EvaluationCriterion("clarity", "Clarity of writing"),
		new EvaluationCriterion("accuracy", "Factual accuracy", Weight: 2d)
	],
	scale);
```

Most rubrics share one scale across every criterion, so `EvaluationCriterion.ScaleOverride` stays
`null`. Set it only when a specific criterion genuinely needs different level wording than the
rubric's shared scale — that criterion's row uses its own scale, every other criterion keeps using
`EvaluationRubric.Scale`.

## Repeatability Replay Modes

"Repeat this playbook" is ambiguous unless the caller declares *which* of three replay modes it
means. `PlaybookReplayContext<TEvidence, TFinding>` makes that declaration explicit and the executor
enforces the corresponding stage behavior:

- **Rerun** (default): run Collect, Evaluate, and Record fresh. This is what happens when no
  `PlaybookReplayContext` is supplied, and it is the correct mode for "has the source changed" or
  "is the evaluator still scoring consistently" — a different result is expected, not an error.
- **Recall**: skip Collect and Evaluate; reuse `PriorEvidence` and `PriorFinding` and run only
  Record. Use this to re-render a materialization from a persisted result without any inference
  cost or drift risk.
- **Replay**: skip Collect, reuse `PriorEvidence`, but re-run Evaluate and Record to verify a
  governed result reproduces exactly against the same evidence.

```csharp
using Goodtocode.Agents.Playbook.Execution;

var executor = new PlaybookExecutor<ReviewRequest, ReviewEvidence, ReviewFinding, ReviewRecord>();

// Rerun (default) — fresh Collect + Evaluate + Record.
var rerun = await executor.ExecuteAsync(
	new DocumentReviewPlaybook(),
	new ReviewRequest("Document content"),
	new PlaybookReplayContext<ReviewEvidence, ReviewFinding>(),
	cancellationToken);

// Recall — rehydrate a prior execution, no inference.
var recall = await executor.ExecuteAsync(
	new DocumentReviewPlaybook(),
	new ReviewRequest("Document content"),
	new PlaybookReplayContext<ReviewEvidence, ReviewFinding>(
		PlaybookReplayMode.Recall,
		SourceExecutionId: "run-123",
		PriorEvidence: rerun.Evidence,
		PriorFinding: rerun.Finding),
	cancellationToken);

// Replay — reuse the prior evidence, re-run Evaluate to verify exact reproduction.
var replay = await executor.ExecuteAsync(
	new DocumentReviewPlaybook(),
	new ReviewRequest("Document content"),
	new PlaybookReplayContext<ReviewEvidence, ReviewFinding>(
		PlaybookReplayMode.Replay,
		SourceExecutionId: "run-123",
		PriorEvidence: rerun.Evidence),
	cancellationToken);
```

`result.Metadata.ReplayMode` and `result.Metadata.SourceExecutionId` are always populated so a host
can persist which mode produced a given execution. `Recall` and `Replay` throw
`InvalidOperationException` if `SourceExecutionId`/`PriorEvidence` (and, for `Recall`,
`PriorFinding`) are missing.

This capability is the CER-level building block for the four governance pillars (observability,
auditability, defensibility, repeatability) described in
[Goodtocode.Agents.Governance](https://github.com/Goodtocode/agents-governance); wiring the four
pillars into playbook execution context is planned as a follow-up.

## Named, Tool-Attributed Steps

`ICollectStep<,>`/`IEvaluateStep<,>`/`IRecordStep<,>` are the pure workflow-stage contracts — what a
stage does. `ICollectStepTool<,>`/`IEvaluateStepTool<,>`/`IRecordStepTool<,>` extend those with a
`ToolName`, because execution is tool-based: a host needs to know *which* registered tool performed
a stage for auditability, tool-selection, and future multi-tool-per-stage scenarios. These are two
different concerns — the stage contract is the workflow definition; the tool contract is the
attributable execution unit — and the tool contracts are a strict extension, so any existing `IEvaluateStep<,>`
implementation is already usable wherever the plain stage contract is expected.

```csharp
public sealed class SqlBlockingCollectTool : ICollectStepTool<SqlBlockingRequest, SqlBlockingEvidence>
{
	public string ToolName => "sql.blocking.collect";

	public Task<SqlBlockingEvidence> ExecuteAsync(
		SqlBlockingRequest input,
		CancellationToken cancellationToken = default) => ...;
}
```

## Observing Stage Activity

`PlaybookExecutor.ExecuteAsync` accepts an optional `IPlaybookStepActivityRecorder<TEvidence, TFinding, TMaterialization>`
so a host can persist or emit observability/auditability evidence after each stage without the
executor knowing about any storage concern. When a stage implementation is also an `*StepTool`, the
recorder is told which tool produced the result; otherwise it receives `null`. No recorder is
required — omitting it is a no-op.

```csharp
var executor = new PlaybookExecutor<ReviewRequest, ReviewEvidence, ReviewFinding, ReviewRecord>();
var result = await executor.ExecuteAsync(
	new DocumentReviewPlaybook(),
	new ReviewRequest("Document content"),
	cancellationToken,
	activityRecorder: myActivityStoreAdapter);
```

## Per-Stage Tool Resolution and Registration

A host that needs to mix deterministic and AI-agent/model-backed tools per stage — or run more than
one tool per stage across different playbooks — can use `Goodtocode.Agents.Playbook.Tools` instead of
hand-rolling keyed DI wiring:

- `PlaybookToolKey` — a validated, normalized value-object key (lowercase letters, numbers, `.`, `_`,
  `-`) identifying a registered tool.
- `[PlaybookTool("your.tool.key")]` — declared on a concrete `*StepTool` implementation so a
  reflection-based scanner can discover and key its registration without constructing an instance.
- `AddPlaybookStepTools<TCollectInput, TEvidence, TEvaluateInput, TFinding, TRecordInput, TMaterialization>()`
  — scans one or more assemblies for attributed tools and registers each with `AddKeyedScoped`. A
  type implementing a stage interface without the attribute, or a duplicate key, fails registration
  explicitly rather than being silently skipped or auto-keyed off a brittle string.
- `IPlaybookStepToolResolver<...>` / `KeyedPlaybookStepToolResolver<...>` — resolves a stage's tool by
  an optional `PlaybookToolKey`, falling back to a host-supplied default key when a playbook's contract
  does not name one.

```csharp
services.AddPlaybookStepTools<ReviewRequest, ReviewEvidence, ReviewEvidence, ReviewFinding, ReviewFinding, ReviewRecord>(
	typeof(Program).Assembly);

services.AddSingleton<IPlaybookStepToolResolver<ReviewRequest, ReviewEvidence, ReviewEvidence, ReviewFinding, ReviewFinding, ReviewRecord>>(
	sp => new KeyedPlaybookStepToolResolver<ReviewRequest, ReviewEvidence, ReviewEvidence, ReviewFinding, ReviewFinding, ReviewRecord>(
		sp,
		defaultCollectKey: PlaybookToolKey.Create("review.deterministic.collect"),
		defaultEvaluateKey: PlaybookToolKey.Create("review.deterministic.evaluate"),
		defaultRecordKey: PlaybookToolKey.Create("review.deterministic.record")));
```

This is the same shape a MAF (or other agentic runtime) workflow adapter uses to resolve each graph
node's tool per execution, so a playbook can freely mix a deterministic Collect, an agentic Evaluate,
and a deterministic Record without the adapter changing.

## Universal Prompt-Based Tools (Goodtocode.Agents.Playbook.Prompting)

Most implementers eventually want a Collect, Evaluate, and Record tool that is 100 percent prompt-driven: the mapping instruction (or, for Evaluate, the rubric) fully describes how to turn the input into the typed output, so the tool implementation itself never changes across playbooks. The optional `Goodtocode.Agents.Playbook.Prompting` package provides exactly that, backed by `Microsoft.Extensions.AI`'s `IChatClient` and its structured-output support:

```powershell
dotnet add package Goodtocode.Agents.Playbook.Prompting
```

```csharp
services.AddSingleton<IChatClient>(myConfiguredChatClient);
services.AddUniversalPromptTools<ReviewRequest, ReviewEvidence, ReviewFinding, ReviewRecord>();
```

- `PromptCollectTool<TRawInput, TEvidence>` — takes a `PromptCollectRequest<TRawInput>(RawInput, MappingInstruction)`; the instruction carries all of the mapping intelligence.
- `PromptEvaluateTool<TEvidence, TFinding>` — takes a `PromptEvaluateRequest<TEvidence>(Evidence, Rubric, AdditionalInstruction)`; the rubric is rendered deterministically by `EvaluationRubricPromptRenderer` before every model call, so the same rubric always produces the same prompt text.
- `PromptRecordTool<TFinding, TMaterialization>` — takes a `PromptRecordRequest<TFinding>(Finding, RecordInstruction)`.

All three are generic over any host-defined evidence/finding/materialization type, so `AddUniversalPromptTools<...>()` is called once per playbook shape, not once per tool. These tools report `DeterministicReplaySupported = false` to a governance recorder, since model output is not guaranteed byte-identical across calls — see [Goodtocode.Agents.Governance](https://github.com/Goodtocode/agents-governance)'s replay modes for how to model that correctly.

## Compatibility


The core package is independent of:

- Microsoft Agent Framework
- Microsoft.Extensions.AI
- Semantic Kernel
- model providers
- persistence and transport formats
- web hosting and worker runtimes

Adapters can call model or tool APIs inside a stage while the playbook contract remains stable.

The optional `Goodtocode.Agents.Playbook.Prompting` package is the one deliberate exception: it
depends on `Microsoft.Extensions.AI` because that is its entire purpose (universal prompt-based
tools). It is a separate NuGet package so a host that wants only the deterministic core never pulls
in a model-client dependency it does not use.

## Development

```powershell
dotnet build Goodtocode.Agents.Playbook.slnx
dotnet test Goodtocode.Agents.Playbook.slnx
```

The repository contains the library in `src/Goodtocode.Agents.Playbook` and focused tests in `src/Goodtocode.Agents.Playbook.Tests`.

## License

MIT. See [LICENSE](LICENSE).

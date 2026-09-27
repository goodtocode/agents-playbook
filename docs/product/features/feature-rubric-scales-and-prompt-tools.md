# Feature: Discrete/Continuous Rubric Scales and Universal Prompt-Based Tools

## Status

Implemented in the core `Goodtocode.Agents.Playbook` package (rubric scale model) and the new
`Goodtocode.Agents.Playbook.Prompting` satellite package (universal prompt-based tools).

## Problem

Two related gaps existed before this feature:

1. `EvaluationRubric.Scale` had one shape (`EvaluationScale`/`EvaluationScaleEntry`, a numeric-band
   scale). It could not express a discrete, ordinal Likert-style rubric where each level carries a
   label and a qualitative description (for example, 0 = Not implemented, 1 = Unsatisfactory, ...,
   5 = Greatly exceeds standards).
2. There was no reusable, universal tool implementers could reach for when they want a Collect,
   Evaluate, or Record stage that is 100 percent prompt-driven: the mapping/scoring instructions
   fully describe the input-to-output mapping, and the tool implementation itself carries no
   playbook-specific logic. Every host had to hand-roll this pattern.

## Design Decisions

### Rubric scale: two shapes, not N rubric-style classes

Rubric pedagogical styles (holistic, analytic, checklist, weighted) are not different data shapes;
they are different usage patterns of the same underlying shape. The only genuinely different shape
is discrete ordinal levels versus continuous numeric ranges. `IEvaluationScale` is the marker
interface for exactly two implementations:

- `DiscreteEvaluationScale(ScaleId, Levels: IReadOnlyList<EvaluationScaleLevel>)` — an ordered set
  of `(Level, Label, Description)`. A holistic rubric is this shape with one criterion; a checklist
  is this shape with two levels; an analytic rubric is this shape shared across many criteria.
- `ContinuousEvaluationScale(ScaleId, Entries: IReadOnlyList<EvaluationScaleEntry>)` — the original
  numeric-band shape (renamed from `EvaluationScale` since it had zero consumers at the time of this
  change), for threshold/range-based scoring.

`EvaluationCriterion.ScaleOverride` is an optional per-criterion override, used only when a specific
criterion's level wording must differ from the rubric's shared `Scale`. Most rubrics leave this
null and share one scale across every criterion.

This is a composition-based design (Open/Closed): a rubric-to-prompt renderer or executor
pattern-matches on `IEvaluationScale`'s two concrete types rather than branching on a rubric-style
enum, and a third genuinely different scale shape (if one is ever needed) is a new type
implementing the interface, not a breaking change to the existing two.

### Universal prompt-based tools: a separate satellite package, not the core

`Goodtocode.Agents.Playbook.Prompting` provides `PromptCollectTool<TRawInput, TEvidence>`,
`PromptEvaluateTool<TEvidence, TFinding>`, and `PromptRecordTool<TFinding, TMaterialization>` —
generic tools where all of the mapping/scoring intelligence lives in the request's instruction text
(or, for Evaluate, an `EvaluationRubric` rendered by `EvaluationRubricPromptRenderer`), not in the
tool implementation. One tool implementation is reusable across every playbook a host defines.

This required a dependency on `Microsoft.Extensions.AI`'s `IChatClient` and structured-output
support (`GetResponseAsync<T>`), which conflicts with the core package's zero-required-dependency
design. Rather than reversing that decision, `Goodtocode.Agents.Playbook.Prompting` is a second,
optional NuGet package in this same repository/solution — a host that wants only the deterministic
core never pulls in a model-client dependency it does not use, while a host that wants the
universal prompt tools adds one more package reference.

### Registration: closed-generic per playbook shape, not open-generic

The tools are generic over any host-defined type, but their request types are wrapper types
(`PromptCollectRequest<TRawInput>`, not `TRawInput` directly). .NET's open-generic DI resolution
requires the implementation's generic parameters to map positionally to the service interface's
generic parameters — it cannot "unwrap" a constructed generic type parameter from a closed service
request. `AddUniversalPromptTools<TRawInput, TEvidence, TFinding, TMaterialization>()` is therefore
a closed-generic registration helper, called once per playbook shape (matching the same convention
already established by `AddPlaybookStepTools<...>()`), not an automatic open-generic registration.

## Testing

- `EvaluationRubricPromptRendererTests` — deterministic rendering of both scale shapes and the
  per-criterion override, with no model call.
- `PromptToolsTests` — each tool against a fake `IChatClient` returning canned structured JSON, no
  live model call.
- `PromptToolRegistrationTests` — DI resolves all three stages for one playbook shape.

## Related

- [README.md](../../README.md) — "Rubric Scales: Discrete and Continuous" and "Universal
  Prompt-Based Tools" sections.
- [Goodtocode.Agents.Playbook.Prompting/README.md](../../src/Goodtocode.Agents.Playbook.Prompting/README.md)

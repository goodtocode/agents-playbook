# Feature: Repeatable Playbook Execution

## Overview
`PlaybookExecutor` runs a Collect → Evaluate → Record playbook once, or repeats a prior run. The repeat semantics come from `Goodtocode.Agents.Governance`, which Playbook references; Playbook only runs the actions the policy plans.

## How it works
1. The caller builds a `PlaybookExecutionRequest` with `Repeat` (a Governance `RepeatRequest`), and `SeedEvidence` / `SeedFinding` from the source run when needed.
2. `PlaybookExecutor` asks `IRepeatPolicy` for a `RepeatPlan`.
3. `Validate` fails only if a skipped action has no seed.
4. Planned actions run; skipped actions use the seeds. `ActionsExecuted` reports the plan as Governance `ActionType` flags (Collect, Evaluate, Record), and checkpoints are keyed by `ActionType`; Playbook defines no action or stage type of its own.
5. The source run id is carried on `PlaybookIdentity` and `Metadata.SourceExecutionId`.

## Modes
Rerun runs all actions, Replay runs Evaluate and Record on seeded evidence, Recall runs none (Record only when the policy option is on).

## Out of scope
Persistence of source runs. The host (for example an application) stores executions and supplies the seeds.

## Tests
`PlaybookRepeatabilityTests` and `PlaybookExecutionRequestTests` cover each mode, seed validation, checkpoints, recorder notification, cancellation and custom policies.

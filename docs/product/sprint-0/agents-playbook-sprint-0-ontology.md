# Agents Playbook - Sprint 0 Ontology

## Sprint 0 Outcome
Sprint 0 establishes the shared language for a host-independent, strongly typed Collect, Evaluate, and Record playbook engine.

## Purpose
Define the terms used in the public contracts, execution flow, tests, and product planning for deterministic playbook execution.

## Canonical Concepts
- Playbook: A stable named and versioned definition of typed CER stages.
- Collect: The first stage, which converts typed input into typed evidence.
- Evaluate: The second stage, which converts evidence into a typed finding.
- Record: The third stage, which converts a finding into a typed materialization.
- Execution: One ordered invocation of a playbook definition by `PlaybookExecutor`.
- Execution Result: The typed evidence, finding, materialization, and `PlaybookExecutionMetadata` returned by an execution.
- Execution Context: Optional `PlaybookExecutionContext` containing knowledge, identity, rubric, and host governance metadata.
- Contextual Stage: An Evaluate or Record stage that additionally implements its contextual contract.
- Policy Evaluation: The validate-then-evaluate flow provided by `PolicyEvaluateDefinitionBase`.
- Deterministic Order: Collect completes before Evaluate, and Evaluate completes before Record.

## Ubiquitous Language (Approved)
Playbook, Stage, Collect, Evidence, Evaluate, Finding, Record, Materialization, Execution, Context, Definition, Deterministic Order.

## Synonyms Rejected
- Workflow payload -> Playbook Execution Result
- Step -> Stage, when referring to a CER contract
- Output -> Materialization, when referring specifically to Record output
- Ambient context -> Explicit Playbook Execution Context

## Sprint 0 Decisions
1. The public abstraction is a typed CER playbook, not a workflow-runtime integration.
2. Execution order is explicit and deterministic.
3. Context is passed explicitly and is opt-in for Evaluate and Record stages.
4. Existing stage contracts remain usable when a context-aware contract is not implemented.
5. A policy evaluator validates evidence before applying policy.

## Traceability
Canonical terms map to `src/Goodtocode.Agents.Playbook/Steps/*` and `src/Goodtocode.Agents.Playbook/Execution/*`.
This document is the ontology baseline for future playbook capabilities.

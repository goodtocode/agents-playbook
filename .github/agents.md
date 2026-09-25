# AI Agent Operating Guide

## Purpose
This file defines repository-specific operating rules for AI agents working in goodtocode/agents-playbook.

## Repository Scope
- Primary deliverable: reusable .NET library package Goodtocode.Agents.Playbook.
- Current solution: Goodtocode.Agents.Playbook.slnx.
- Main projects:
  - src/Goodtocode.Agents.Playbook/ (library)
  - src/Goodtocode.Agents.Playbook.Tests/ (tests)

## Required Reading Order
1. .github/copilot-instructions.md
2. README.md
3. docs/product/sprint-0/agents-playbook-sprint-0-ontology.md
4. docs/product/sprint-0/agents-playbook-sprint-0-context-diagram.md

## Agent Workflow
1. Confirm target behavior from product docs.
2. Keep changes minimal and inside the requested scope.
3. Preserve the typed CER execution API and deterministic behavior.
4. Update or add tests when behavior changes.
5. Verify with build/test before completion.

## Constraints
AI agents must not:
- Introduce repo structure assumptions from other solutions.
- Break existing package contracts without explicit request.
- Add unrelated dependencies or architecture layers.
- Edit unrelated workflows/docs outside task scope.

## Delivery Expectations
- Changes compile.
- Tests pass for affected areas.
- Docs remain consistent with code.
- File paths and naming match this repository.

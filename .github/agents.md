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

## Source Control Workflow
- Leave all changes local and uncommitted. Do not run `git commit`, `git push`, or open pull requests; the maintainer reviews the uncommitted diff first and then commits, pushes, and opens the PR.
- Do not create or switch branches unless asked. Summarize what changed so the diff can be reviewed.

## NuGet Versioning
- Do not alter `<Version>`, `<FileVersion>`, `<AssemblyVersion>`, `<InformationalVersion>`, or pin temporary versions (for example `9.9.0-local`) in any `.csproj`, and do not force version bumps.
- Versioning is owned by `.github/scripts/ci/Set-Version.ps1` and `Get-Version.ps1`, driven by `VERSION_MAJOR`/`VERSION_MINOR` in the workflows. Only the maintainer changes those, and only when asked.
- For local cross-repo testing, do not edit project files; ask the maintainer how to consume a local build.

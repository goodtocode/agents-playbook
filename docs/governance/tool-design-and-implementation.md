# Tool Design and Implementation Governance

## Purpose
Define universal, project-agnostic standards for building AI tools that respect Clean Architecture boundaries.

## Scope
Applies to all tool implementations, regardless of domain, can, or feature area.

## Core Principles
- **Single responsibility**: tools orchestrate; application/domain execute business behavior.
- **Boundary-first**: tool code must call application commands/queries, never persistence directly.
- **Deterministic contracts**: tool inputs/outputs must be explicit and stable.
- **Policy by default**: authorization, tenant scope, and ownership checks happen through application handlers.
- **Observable execution**: each tool invocation is traceable, measurable, and debuggable.

## Layering Rules

### Tool Layer Responsibilities
- Validate tool input shape and required arguments.
- Translate tool intent into application requests.
- Invoke application requests through a dedicated execution gateway.
- Shape output for conversational consumption.

### Tool Layer Prohibitions
- No direct `DbContext`/ORM/repository usage.
- No direct SQL or persistence entity access.
- No business rule ownership in tools.
- No bypass of application validation/guard/pipeline behaviors.

### Application Layer Responsibilities
- Own all business use cases and state changes.
- Enforce invariants, policies, and scoped access.
- Return typed outcomes and explicit failures.

## Standard Tool Pattern

### Required Building Blocks
- A shared tool base class for scoped execution concerns.
- A shared application execution gateway abstraction.
- Tool methods that call command/query requests through the gateway.

### Required Invocation Path
`Tool Method -> Tool Execution Gateway -> Mediator Pipeline -> Command/Query Handler -> Domain/Infrastructure`

## Request and Response Contracts

### Request Standards
- Use typed command/query request objects.
- Include user/tenant context via standard request contracts.
- Avoid dynamic, weakly typed payloads for internal execution.

### Response Standards
- Return stable, typed structures from application handlers.
- Map responses to tool-friendly shape without changing semantics.
- Preserve explicit error outcomes (validation, not found, conflict, forbidden).

## Security and Access Control
- All scoped operations must flow through handlers that enforce user context.
- “My” operations enforce owner + tenant.
- “Our” operations enforce tenant.
- Tools must not implement ad hoc policy logic that duplicates handler rules.

## Error Handling
- Do not swallow exceptions.
- Convert expected domain/application failures into explicit tool outcomes.
- Keep failure semantics consistent across tools.
- Avoid success-shaped fallbacks for failed operations.

## Side-Effect Governance
- Read operations should be side-effect free.
- Mutating operations should be explicit and auditable.
- For high-impact writes, support preview/confirmation patterns where applicable.

## Dependency Injection and Lifetime
- Register tool dependencies in composition root only.
- Keep dependency graphs explicit and testable.
- Use scoped execution for request handling dependencies.
- Prefer one shared execution abstraction over per-tool mediator/service locator patterns.

## Observability and Audit
- Log tool name, operation name, correlation ID, duration, and outcome.
- Capture request/response metadata appropriate for diagnostics.
- Record audit-relevant events for mutating operations.

## Testing Standards

### Unit Tests
- Validate tool argument mapping and output shaping.
- Validate failure mapping behavior.

### Integration Tests
- Verify tool -> gateway -> mediator -> handler flow.
- Verify architecture guardrails (no direct persistence usage in tools).
- Verify authorization and scoped data behavior.

## Architecture Guardrails (Required)
- Enforce with tests or analyzers:
  - no ORM/persistence references in tool classes
  - no direct mediator/service locator bypass when a shared gateway exists
  - inheritance/composition pattern compliance for tool base abstractions

## Change Management

### When adding a new tool
1. Define command/query contract first.
2. Implement handler validation and policy enforcement.
3. Implement tool method via execution gateway.
4. Add or update integration tests and architecture guard tests.
5. Validate build and targeted tests.

### When refactoring existing tools
1. Inventory direct persistence access and policy duplication.
2. Replace with command/query dispatch through shared gateway.
3. Preserve observable behavior and response compatibility.
4. Add regression tests and remove bypass code.

## Anti-Patterns
- Tool directly querying or mutating storage.
- Tool implementing domain invariants.
- Tool bypassing application request pipeline.
- Broad catch-and-ignore behavior.
- Undocumented output shape changes.

## Definition of Done
A tool implementation is complete only when:
- It uses the shared execution pattern.
- It has no direct persistence dependency.
- Policy and validation are enforced through application handlers.
- It has targeted tests and architecture guard compliance.
- It is observable and auditable.

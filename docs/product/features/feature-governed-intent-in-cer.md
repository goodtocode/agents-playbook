# Feature: Governed Agentic Intent in CER

## Summary
Playbook decides **where** the governance Instruction and Preamble apply across Collect, Evaluate and Record. Governance defines **what** they are (`IntentPolicy`, `IGovernancePreambleComposer`); playbook holds no policy text.

## Contracts (`Goodtocode.Agents.Playbook.Prompting.Governance`)
- `GovernedChatClient`: `DelegatingChatClient`, the single shared seam. No per-tool application.
- `GovernedChatOptionKeys.ForTurn(GovernedTurnKind)`: how a tool tags its stage. The universal Collect, Evaluate and Record tools do this.
- `IGovernedIntentScope` / `GovernedIntentScope`: tenant, jurisdiction and the once-per-run Instruction.
- `GovernedChatClientOptions`: `IntentRequired`, `PreambleMode`, `PreambleTurns` (per-stage opt-out; default all).
- `IGovernedTurnObserver` / `GovernedTurnRecord`: policy id, version, hash, and whether Instruction and Preamble were applied, per call.
- `UseGovernedIntent(...)` on `ChatClientBuilder`, and `AddGovernedIntentPreamble()`.

## Behavior
1. Resolve the policy for the scope. If none resolves and `IntentRequired`, throw `GovernanceValidationException` before any model call.
2. Instruction (scope Instruction passed through the prohibited-fragment guard, then the intent block) becomes the first system message.
3. Preamble is inserted immediately before the final message when the turn is in `PreambleTurns`. Non-LLM steps never reach the client, so they are unaffected.
4. The observer receives the turn record; the caller's message list is not modified.

## Pillar mapping
- Observability: per-turn record of policy identity.
- Auditability: scope tenant plus recorded policy id, version and hash.
- Repeatability: same inputs and policy hash produce the same Instruction and Preamble text; a policy change is drift.
- Defensibility: the Instruction requires citing principle ids with a short justification.

## Limits
A decorator governs calls that go through it. Hosts should register the governed client as the default `IChatClient`; an ungoverned client should be an explicit, auditable choice.

## Typed Instruction on the definition
`IPlaybookSteps.Instruction` is an optional `AgentInstruction?` (default `null`, scope `Playbook`). Hosts build `GovernedIntentScope(tenantId, jurisdiction, definition.Instruction)` from it. `GovernedIntentScope.Instruction` is the governance `AgentInstruction`.

## Step metadata (breaking contract change)
`IPlaybookStepActivityRecorder.RecordCollect/Evaluate/Record` now take `IReadOnlyList<GovernedTurnRecord> governedTurns` before the cancellation token. `PlaybookExecutor(IRepeatPolicy, GovernedTurnLedger? turns = null)` drains the ledger after each action and passes the turns to the recorder. Register the same `GovernedTurnLedger` as the `IGovernedTurnObserver` for the run. Turn records (and `GovernedTurnRecord`, `IGovernedTurnObserver`) now live in governance.

## Verification (Phase D)
Use governance `IIntentOutputVerifier` on the Evaluate or Record output, and call `EnsurePassed()` to fail closed. Playbook adds no verifier of its own.

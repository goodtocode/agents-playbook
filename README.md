# Goodtocode.Agents.Playbook

Strongly typed Collect, Evaluate, and Record (CER) playbook contracts with a deterministic execution engine.

The package defines a small host-independent execution boundary. A playbook supplies typed stages, and `PlaybookExecutor` invokes them in order while returning typed outputs and execution metadata. Evaluate and Record stages may optionally consume explicit knowledge, identity, rubric, and governance metadata through `PlaybookExecutionContext`.

The core package is independent of Microsoft Agent Framework, persistence, and transport formats. Hosts can execute the same typed playbook deterministically or adapt it to a workflow runtime.

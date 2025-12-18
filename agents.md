---
name: piper-pipeline-architect
description: Canonical architectural constraints for the Piper in-process pipeline substrate.
---

# Piper Pipeline Substrate — Agent Rules

You are assisting with code changes, refactors, or extensions to the Piper pipeline.

These rules are **non-negotiable**.

If a request conflicts with them, you must **refuse or redirect**.

---

## 1. Purpose (Authoritative)

This pipeline exists to provide a **small, explicit, in-process execution substrate** for **bounded, concurrent, data-flow processing**.

Its sole responsibility is to:
> Move data through a **statically defined graph of elements** efficiently, predictably, and observably.

It is **not** a workflow engine, stream-processing framework, or business-logic abstraction.

---

## 2. Core Architectural Constraints

You MUST ensure that all generated code preserves the following:

- **In-process execution only**
- **Push-based streaming** (no pull/lazy evaluation)
- **Static pipeline graph** (no runtime mutation)
- **Explicit topology** (elements, pads, links)
- **Bounded memory and backpressure by default**
- **Per-element parallelism**, not pipeline-global
- **Clear separation of definition and runtime**
- **Observability via events, not control-flow exceptions**

---

## 3. Explicitly Forbidden Concepts

You MUST NOT introduce:

- Pipeline-level conditionals (`if/else`, branching logic in builder/runtime)
- Dynamic graph mutation
- DSLs or expression languages
- Business or domain logic
- Distributed execution concepts
- Scheduling frameworks
- Implicit wiring or auto-routing
- Unbounded queues or buffers
- Hidden control flow

If a feature requires any of the above, it is **out of scope**.

---

## 4. Routing & Branching Rules

- **All routing decisions belong inside elements**
- Tee, merge, filter, and router behaviors MUST be modeled as elements
- The pipeline graph remains static at all times
- Pads and links MUST be declared explicitly

---

## 5. Error Handling Rules

- Errors MUST be propagated as **data or events**
- Exceptions MUST NOT be used for normal control flow
- Element failures MUST NOT crash the entire pipeline by default
- The pipeline runtime may transition to `Faulted`, but must do so predictably

---

## 6. Performance Rules

You MUST:

- Avoid reflection in hot execution paths
- Avoid per-item allocations
- Avoid string-based lookups during streaming
- Bind channels, readers, and writers once during preparation
- Use `Channel<T>` with bounded capacity for all inter-element communication

Performance regressions are considered **architectural violations**.

---

## 7. Validation Rules

Before considering code complete, you MUST ensure:

- All required pads are connected
- Pad type compatibility is validated
- Illegal fan-in/fan-out is rejected
- One-writer-per-output-pad invariants hold
- Pipeline definition can be inspected without executing it

---

## 8. Scope Guardrail

If a proposed change requires:

- Workflow semantics
- Conditional pipelines
- Transactions
- Time windows
- Exactly-once guarantees
- Stateful orchestration

You MUST reject the change and suggest building it **on top of** the pipeline instead.

---

## 9. One-Sentence Rule (Override All Others)

> The pipeline moves data — it does not make decisions.

If in doubt, default to **explicit, boring, minimal design**.

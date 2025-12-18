# Pipeline Substrate — Why This Exists

## Purpose

This pipeline exists to provide a **small, explicit, in-process execution substrate** for *bounded, concurrent, data-flow processing*.

It is **not** a general workflow engine, not a stream-processing framework, and not a business-logic abstraction.

Its sole responsibility is to **move data through a statically defined graph of elements efficiently, predictably, and observably**.

---

## What This Solves

This pipeline solves problems where:

* Data flows through **independent stages**
* Stages can run **concurrently**
* **Backpressure and bounded memory** matter
* Partial failures must not crash the whole process
* The execution graph should be **explicit and inspectable**

Typical examples:

* File ingestion
* ETL-style transformations
* Validation pipelines
* Batch processing
* Dependency resolution
* Analysis and reporting pipelines

---

## What This Is (By Design)

* **In-process**
* **Push-based**
* **Statically defined**
* **Type-aware**
* **Bounded**
* **Explicit topology (pads + links)**
* **Separation of definition and runtime**
* **Observable via events, not exceptions**

---

## What This Is *Not*

This pipeline is **not**:

* A workflow engine
* A rule engine
* A state machine
* A DSL
* A business orchestration framework
* A distributed system
* A scheduling framework
* A replacement for async/await
* A place for domain logic

If you need:

* Dynamic graph mutation
* Conditional logic expressed in the pipeline
* Transactions
* Long-running stateful processes
* Time windows
* Exactly-once guarantees

**This is the wrong tool.**

---

## Core Architectural Rules (Non-Negotiable)

1. **The pipeline graph is static.**
   No runtime linking, unlinking, or mutation.

2. **Routing decisions belong inside elements.**
   The pipeline never contains conditionals.

3. **Elements define behavior.**
   The pipeline defines wiring and lifecycle only.

4. **Errors are data or events, not control flow.**
   Exceptions must not tear down the pipeline.

5. **Backpressure is enforced by default.**
   Unbounded queues are a bug.

6. **Nothing here knows business semantics.**

---

## Why We Did Not Use an Existing Framework

Existing solutions either:

* Assume media processing (GStreamer)
* Assume distributed execution (Beam, Flink, Storm)
* Hide the graph (TPL Dataflow, Rx)
* Require DSLs or heavy abstractions

This pipeline exists to cover the **missing middle**:

> A small, honest, inspectable, in-process dataflow substrate.

---

## Guardrail Against Scope Creep

If a proposed change requires:

* Adding “if / else” to the pipeline
* Introducing a DSL
* Making topology implicit
* Encoding business rules
* Supporting distributed execution

**Reject it.**

Build it *on top* instead.

---

## One-Sentence Summary

> This pipeline moves data — it does not make decisions.

---

If you want, I can:

* Turn this into a README badge-level summary
* Add a “when NOT to use this” checklist
* Or help you freeze a minimal API surface to protect this long-term

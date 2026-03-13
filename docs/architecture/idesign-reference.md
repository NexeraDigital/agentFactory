# IDesign Method Reference

*Based on Juval Löwy's IDesign methodology and "Righting Software"*

---

## Part I — System Design

### The Prime Directive

**Never design against the requirements.** Requirements will change; the architecture should not have to.

- Do NOT reflect requirements/features/use-cases directly into architecture structure
- Design to survive requirement change
- Features emerge from integrating stable building blocks, not from dedicated "feature boxes"

### Step 1 — Validate with Use Cases

Use cases validate the architecture, they don't drive it.

- Start with top 4-6 distinct use cases
- Include present/future considerations
- Stop when additional effort yields little new architecture insight
- Expect iterative factoring as understanding improves

### Step 2 — Decompose Based on Volatility

Instead of mapping requirements A, B, C into services A, B, C, identify areas of potential change.

**Two Volatility Axes:**

| Axis | Question |
|------|----------|
| Same customer over time | What changes for a single customer as time passes? |
| Same time across customers | What differs across customers at the same point in time? |

**Rules:**
- Keep axes independent and encapsulated from each other
- If axes are not independent, you've likely slipped into functional decomposition
- Don't "encapsulate the nature of the business" as volatility—that's speculative

### Step 3 — Reject Functional Decomposition

Functional decomposition (building components from ordered steps of use cases) maximizes change impact.

**Smells:**
- **Forks**: Too many tiny services; clients stitch them together
- **Staircases**: Bloated services with downstream chaining and compensation
- **Client coupling**: Clients calling multiple managers for a single use case
- **Multiple entry points**: Duplicated behavior across services

### Step 4 — Apply the Layered Template

```
┌─────────────────────────────────────┐
│           CLIENTS (Who)             │  ← Users or other systems
├─────────────────────────────────────┤
│          MANAGERS (What)            │  ← Use-case workflows (nouns)
├─────────────────────────────────────┤
│           ENGINES (How)             │  ← Business rules (verbs, rare)
├─────────────────────────────────────┤
│       RESOURCE ACCESS (Where)       │  ← Data/resource encapsulation
├─────────────────────────────────────┤
│     RESOURCES & UTILITIES           │  ← Infrastructure, databases
└─────────────────────────────────────┘
```

**Layer Responsibilities:**

| Layer | Encapsulates | Notes |
|-------|-------------|-------|
| Client | Client technology volatility | Single point of entry preferred |
| Manager | Use-case/workflow sequencing | Collection of related use cases; near-expendable |
| Engine | Business rules and activities | Reusable across managers; somewhat rare |
| Resource Access | Access to resources | Business verbs, not CRUDs; shareable |
| Resources | Physical resources | Databases, external systems |
| Utilities | Common infrastructure | Callable by all layers |

### Step 5 — Enforce Closed Architecture

| Type | Rules | Use |
|------|-------|-----|
| **Closed** (default) | Call only the layer immediately below; no sideways calls | Standard; maximum encapsulation |
| Semi-open | Call any layer beneath, but not up/sideways | Sparingly; trades encapsulation for flexibility |
| Open | Can call anyone | Avoid; undermines layers |

### Step 6 — Define Components

**Naming Conventions (Smell Checks):**
- Managers are **nouns**. Verb/adverb names indicate functional decomposition.
- Engines are **verbs**. Noun names indicate misclassification.
- Don't create engines unless they encapsulate meaningful reusable activity.

**Effort Distribution:**
- Managers should be near-expendable (workflow changes map here)
- Bulk effort: engines, resource access, resources, clients/UI, utilities

### Step 7 — Interaction Rules

| Rule | Description |
|------|-------------|
| Single manager per use case | Clients don't call multiple managers (couples managers via client) |
| Share engines/resource access | Reuse across managers is encouraged |
| Manager-to-manager queuing | Law of 0, 1, ∞: don't queue to more than one manager; use pub/sub if recipients vary |
| No queuing to engines/RA | Engines and resource access are synchronous |
| Engines don't publish/subscribe | Engines don't call each other |
| Resource access doesn't publish | Resource access services don't call each other |

### Step 8 — Diagram Notation

| Symbol | Meaning |
|--------|---------|
| Black arrow | Synchronous call |
| Gray dashed arrow | Queued call |
| Solid bar | Authentication boundary |
| Patterned bar | Authorization boundary |
| Boxed participants | Transaction boundary |

### Step 9 — Synchronization Analysis

- Identify the logical thread of execution
- Re-entrant cycles indicate deadlock risk or poor design
- Use to justify queuing/async when appropriate

### Step 10 — Allocation Diagrams

Capture decisions with simple diagrams (one concern per diagram):
- Service allocation
- Assembly allocation
- Runtime process allocation
- Identity groupings
- Auth boundaries (authentication/authorization)
- Transaction boundaries
- Instance management (per-call, sessionful, singleton)
- Synchronization view

### Step 11 — Build Vertical Slice

- Build end-to-end slice early to validate feasibility
- Flush out technology constraints
- Refine architecture based on findings

---

## Part II — Project Design

Project design is an engineered plan derived from the architecture.

### Step 12 — Core Team

| Role | Responsibility |
|------|---------------|
| Architect | Owns system design AND project design |
| Project Manager | Shields, tracks, reports |
| Product Manager | Proxy for customers |

### Step 13 — Project Network

1. List activities needed to implement the architecture
2. Estimate with reasonable accuracy (use duration buckets or high/low/expected)
3. Create dependency graph (network diagram)
4. Identify critical path; calculate project duration

### Step 14 — Use Float

- Compute float (slack) on non-critical activities
- Use float for staffing and scheduling flexibility
- Low float = fragility; more float = better variance absorption

### Step 15 — Assign Resources and Cost

- Network topology drives duration (individual estimates average out)
- Calculate cost after staffing decisions
- Recognize time/cost tradeoffs

### Step 16 — Engineer Options

**There is no single "THE Project"—produce multiple viable options:**

| Option Type | Characteristic |
|-------------|---------------|
| Normal | Baseline balanced approach |
| Fastest | Aggressive schedule; higher cost/risk |
| Least expensive | Longer duration; lower cost |
| Safest | Lowest risk; may cost more or take longer |

- Quantify and compare risk across options
- Present only good options—any choice should be defensible

### Step 17 — Execution and Tracking

- Define status conventions
- Track earned value/progress
- Use projections and corrective actions

---

## Data Passing Rules

Between layers, pass ONLY:
- Primitives
- Arrays of primitives
- Data contracts
- Arrays of data contracts

**Never:**
- Leak entities across boundaries
- Share logic behind data contracts across layers
- Each layer interprets data in its own context

---

## Checklists

### System Design Checklist

- [ ] Avoided reflecting requirements into component structure (Prime Directive)
- [ ] Decomposed around volatility (time/customer axes), not use-case steps
- [ ] Structure avoids forks and staircases ("stick figure" decomposition)
- [ ] Architecture is closed by default
- [ ] Managers are near-expendable, focused on workflow
- [ ] Engines/resource access carry reusable logic
- [ ] Clients call only one manager per use case
- [ ] Validated with small set of distinct use cases

### Project Design Checklist

- [ ] Plan derived from architecture (system assembly instructions)
- [ ] Network diagram with dependencies and critical path
- [ ] Duration calculated from network; float used for scheduling
- [ ] Multiple viable options with schedule/cost/risk tradeoffs
- [ ] Risk and float treated as first-class planning variables
- [ ] Tracking plan defined (status, earned value, projections)

---

## Quick Reference: What Goes Where

| Component Type | Named As | Calls | Called By |
|---------------|----------|-------|-----------|
| Client | (varies) | 1 Manager | Users/systems |
| Manager | Noun | Engines, RA | Clients, other Managers (queued) |
| Engine | Verb | Resource Access | Managers |
| Resource Access | Noun | Resources | Managers, Engines |

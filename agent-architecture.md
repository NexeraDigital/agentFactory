# Agent Architecture Methodology

A reusable methodology for establishing a team of specialized AI agents that provide consistent quality, institutional knowledge, and multi-perspective review across software projects. Designed for solo developers or small teams using Claude Code.

## Philosophy

Software quality degrades when a single perspective drives all decisions. A solo developer writing code is also the architect, the security reviewer, the UX designer, and the operations engineer — but they can't hold all those lenses simultaneously.

This methodology solves that by defining **specialized agents**, each with a distinct lens through which they view the same codebase. Every code change gets examined through multiple perspectives, catching issues that a single viewpoint would miss.

The agents are not decorative. They are opinionated, they have hard rules, and they participate at defined workflow stages. They build institutional memory across sessions so knowledge compounds rather than resets.

---

## The Agent Roster

Ten agents organized into three tiers based on when and how they engage. Code cleanliness enforcement has been promoted to **rules** (`.claude/rules/`) that auto-load based on file type, backed by a **PostToolUse prompt hook** that reviews every edit against loaded rules. This three-layer architecture (rules → hooks → agents) provides continuous enforcement without consuming an agent invocation.

### Tier 1: Always-On Architects (Planning + Review)

These agents participate in **every planning session** and sit on the **post-completion review team**.

#### Backend Architect
- **Role:** Senior backend engineer and code designer
- **Lens:** Code Design & Patterns
- **Core Question:** *"Is this well-designed and following our architectural patterns?"*
- **Scope:** All server-side code — APIs, background jobs, services, data access, infrastructure layer
- **Owns:** Gang of Four patterns, SOLID principles, error handling strategy, async patterns, API design
- **Key Behavior:** Designs code before it's written. Reviews code after it's written. Refuses silent fallbacks — failures must be explicit.
- **Planning Role:** Proposes backend approach, identifies which architectural layers are affected
- **Review Role:** Validates code quality, pattern usage, and architectural compliance
- **Exits Review When:** Changes are frontend-only with zero backend impact

#### React Architect
- **Role:** Senior frontend architect
- **Lens:** Frontend Structure & Patterns
- **Core Question:** *"Is this component architecture sound, maintainable, and performant?"*
- **Scope:** All frontend code — components, hooks, state management, routing, data flow, TypeScript types
- **Owns:** Component hierarchy, smart/presentational separation, state proximity, hook composition, TypeScript strictness
- **Key Behavior:** Focuses on structural code decisions. Does NOT own visual aesthetics (that's Modern UI Agent). Recommends libraries only when complexity warrants them.
- **Planning Role:** Designs component architecture, data flow, state management approach
- **Review Role:** Validates React patterns, component boundaries, state management, TypeScript usage
- **Exits Review When:** Changes are backend-only with zero frontend impact

#### Sentinel
- **Role:** Senior security research agent
- **Lens:** Security & Data Protection
- **Core Question:** *"Is this safe, secure, and protecting user data at every layer?"*
- **Scope:** Full stack — API endpoints, frontend components, auth flows, data access, integration points, infrastructure
- **Owns:** OWASP Top 10 compliance, CWE awareness, auth/authorization enforcement, data sovereignty across call chains, input validation, output encoding
- **Key Behavior:** Traces data flow from entry point to storage and back. Verifies user scoping at every layer boundary. Flags auth gaps, data leakage, and injection vectors.
- **Planning Role:** Identifies security considerations early — auth requirements, data sensitivity, attack surface
- **Review Role:** Audits for vulnerabilities, authorization bypasses, data exposure, and OWASP compliance
- **Never Exits:** Reviews every change. Security is always relevant.

### Tier 2: Specialized Architects (Planning + Authoring)

These agents participate in **planning sessions** when their domain is affected and guide work **during authoring**.

#### Cloud Infrastructure Architect (Azure / AWS / GCP)
- **Role:** Cloud deployment and infrastructure architect
- **Lens:** Infrastructure & Deployment
- **Core Question:** *"What does this code change mean for our deployed environment?"*
- **Scope:** Cloud resources, IaC templates, CI/CD pipelines, secrets management, networking, monitoring
- **Owns:** Resource topology, deployment pipelines, environment configuration, secrets inventory, cost optimization, migration readiness
- **Key Behavior:** Must be advised of ANY change that affects deployment — new resources, config values, secrets, integration points. Maintains a self-update protocol: when it discovers infrastructure changes, it recommends updating its own agent definition to stay current.
- **Planning Role:** Ensures infrastructure implications are considered for every feature
- **Review Role:** Reviews infrastructure-affecting changes (new dependencies, config, cloud resources)
- **One Agent Per Cloud Provider:** Create a separate agent for each cloud provider in use. For Azure projects, use `azure-architect`. For AWS, create `aws-architect`. For multi-cloud projects, run both — they each own their provider's infrastructure. The template library includes `azure-architect.md` as the default; duplicate and adapt for other providers as needed.

#### Structural Architect
- **Role:** Software architecture compliance auditor
- **Lens:** Structural Architecture & Layer Separation
- **Core Question:** *"Are our layers, boundaries, and dependency chains correct?"*
- **Scope:** The overall architectural paradigm — layer definitions, dependency rules, service boundaries
- **Owns:** Architectural methodology enforcement (e.g., IDesign, Clean Architecture, Hexagonal, Vertical Slices), forbidden dependency patterns, layer responsibility definitions
- **Key Behavior:** This is the all-up architect who controls the structural methodology. It doesn't care about code style or security — only that the architectural skeleton is sound and the rules are followed.
- **Planning Role:** Validates that proposed features respect architectural boundaries
- **Review Role:** Audits new classes for layer compliance and forbidden dependencies
- **Adaptation Note:** The generic name is "Structural Architect." Rename and configure for your chosen architectural paradigm. Inject the specific rules (e.g., IDesign's Manager/Engine/Accessor layers, Clean Architecture's dependency rule, etc.).

#### SQL Data Architect
- **Role:** Relational database design and lifecycle specialist
- **Lens:** Relational Data Modeling & SQL Performance
- **Core Question:** *"Is this schema normalized correctly, queryable efficiently, and evolvable safely?"*
- **Scope:** Schema design, migration management, query optimization, indexing strategy, multi-tenancy patterns, EF Core/Dapper data access patterns
- **Owns:** 3NF schema design, migration strategy (expand-contract), clustered/covering/filtered indexes, N+1 prevention, keyset pagination, CQRS read/write split, multi-tenancy with RLS
- **Key Behavior:** Designs schemas before they're built. Reviews queries for performance anti-patterns. Validates migrations for zero-downtime safety. Refuses GUIDs as clustered primary keys, EAV tables, and `Database.Migrate()` at startup.
- **Planning Role:** Designs schema, identifies migration strategy, selects data access pattern
- **Review Role:** Audits for N+1 queries, missing indexes, unsafe migrations, anti-patterns
- **Exits Review When:** Changes have zero relational database impact

#### Table Storage Data Architect
- **Role:** NoSQL/key-value data modeling and Azure Table Storage specialist
- **Lens:** Access-Pattern-Driven Data Design & Partition Strategy
- **Core Question:** *"Are the partition keys designed for the actual query patterns, and is the data denormalized to avoid scans?"*
- **Scope:** Partition key design, row key design, denormalization strategy, query optimization, batch operations, cost management, Cosmos DB Table API migration readiness
- **Owns:** Partition key strategy, row key design (compound keys, reverse-tick, zero-padded), denormalization patterns (intra/inter-partition secondary indexes), query hierarchy enforcement, EGT batch operations, continuation token handling
- **Key Behavior:** Requires access patterns before designing tables. Enforces the query performance hierarchy (point > range > partition scan; table scans are forbidden). Rejects relational patterns (JOINs, normalization, foreign keys). Designs for Table Storage constraints to maintain Cosmos DB portability.
- **Planning Role:** Defines partition strategy based on access patterns, identifies denormalization needs
- **Review Role:** Audits for hot partitions, table scans, missing continuation token handling, relational anti-patterns
- **Exits Review When:** Changes have zero Table Storage / Cosmos DB Table API impact

#### Modern UI Agent
- **Role:** Visual design and UX specialist
- **Lens:** Visual Design & User Experience
- **Core Question:** *"Does this look and feel like a professionally designed product?"*
- **Scope:** Visual aesthetics, design systems, component styling, layout, typography, color, spacing, interaction design
- **Owns:** Design system foundation (colors, typography, spacing, shadows), component visual patterns, hover/loading/empty states, accessibility baseline
- **Key Behavior:** Eliminates "template smell" — the hallmarks of generic AI-generated or default-framework UIs. Opinionated about neutral-first palettes, typographic hierarchy, and strategic accent color usage.
- **Authoring Role:** Guides visual decisions during frontend development
- **Not a Reviewer:** Invoked during authoring when visual/aesthetic concerns arise, not on every review

### Tier 3: On-Demand Specialists

These agents are invoked when specific situations arise.

#### Data Clarifier
- **Role:** Information architecture and data visualization specialist
- **Lens:** Information Architecture & Cognitive Load
- **Core Question:** *"Can users actually make sense of this data and act on it?"*
- **Scope:** Data-heavy UI pages, tables, dashboards, search results, inventory views
- **Owns:** Progressive disclosure hierarchy, cognitive load reduction, data narrative structure, "wall of data" remediation
- **Key Behavior:** Audits data density, defines the "5-second test" (what should a user grasp instantly), proposes Level 1/2/3 information hierarchy. First instinct is always to subtract, not add.
- **Invoke When:** Building or redesigning pages that display dense or complex data sets (large inventories, multi-metric dashboards, search results with many columns)

#### Debug Investigator
- **Role:** Scientific debugging specialist
- **Lens:** Root Cause Analysis
- **Core Question:** *"What is actually happening here, and why?"*
- **Scope:** Any bug, error, unexpected behavior, or test failure across the entire stack
- **Owns:** Seven-phase scientific debugging methodology — observation, hypothesis generation, evidence collection, root cause identification, solution design, implementation, post-mortem
- **Key Behavior:** NEVER modifies code until the root cause is conclusively identified. Generates multiple competing hypotheses before investigating any single one. Other agents defer to Debug Investigator when they need to understand why something is broken.
- **Invoke When:** Any bug, error, test failure, or unexpected behavior is encountered

---

## Workflow Stages

### Stage 1: Planning

**Participants:** All Tier 1 agents + relevant Tier 2 agents + Sentinel

Before writing code, the planning quorum reviews the proposed work:

| Agent | Planning Contribution |
|-------|----------------------|
| Backend Architect | Backend approach, layer impact, pattern selection |
| React Architect | Component architecture, state management, data flow |
| Structural Architect | Architectural compliance, layer boundaries |
| Infrastructure Architect | Deployment impact, new resources/config needed |
| SQL Data Architect | Schema design, migration strategy, query patterns (if SQL involved) |
| Table Storage Architect | Partition strategy, denormalization, access patterns (if Table Storage involved) |
| Sentinel | Security considerations, auth requirements, attack surface |
| Modern UI Agent | Visual approach (if UI work involved) |

### Stage 2: Authoring

**Active agents depend on what's being written:**

| Work Type | Active Agents |
|-----------|---------------|
| Backend code (C#, etc.) | Backend Architect (designs + guides) |
| Data layer (SQL) | SQL Data Architect (schema + queries) + Backend Architect (code quality) |
| Data layer (Table Storage) | Table Storage Architect (partition + keys) + Backend Architect (code quality) |
| Frontend code (React, etc.) | React Architect (structure) + Modern UI Agent (aesthetics, when needed) |
| Infrastructure (IaC, CI/CD) | Infrastructure Architect |
| Full-stack feature | All of the above, filtered by which data persistence is involved |

### Stage 3: Post-Completion Review (Mandatory)

**Every code completion event triggers the review team:**

| Agent | Review Focus |
|-------|-------------|
| Backend Architect | Code quality, patterns, architectural compliance (exits if frontend-only) |
| React Architect | Component patterns, state management, TypeScript (exits if backend-only) |
| Sentinel | Security vulnerabilities, auth, data protection (always reviews) |
| *Rules + Hook* | *Code cleanliness (method length, cohesion, coupling, nesting) enforced automatically via `.claude/rules/` and PostToolUse hook* |

Reviewers run in **parallel**. Each produces independent findings. If an agent has nothing relevant to say, it exits with no findings. Code cleanliness is enforced continuously through rules and hooks rather than a dedicated review agent — use the `/review-cleanliness` skill for batch review of staged changes.

### Stage 4: On-Demand

| Trigger | Agent |
|---------|-------|
| Dense data UI problem | Data Clarifier |
| Bug, error, test failure | Debug Investigator |

---

## RACI Chart

R = Responsible (does the work), A = Accountable (owns the outcome), C = Consulted (provides input), I = Informed (needs to know)

| Activity | Backend Arch | React Arch | Structural Arch | Infra Arch | SQL Data Arch | Table Storage Arch | Sentinel | Modern UI | Data Clarifier | Debug Inv |
|----------|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| New backend feature (SQL) | R | I | C | C | C/R | — | C | — | — | — |
| New backend feature (Table Storage) | R | I | C | C | — | C/R | C | — | — | — |
| New frontend feature | I | R/A | — | — | — | — | C | C | — | — |
| New full-stack feature | R | R | C | C | C* | C* | C | C | — | — |
| Infrastructure change | I | — | — | R/A | — | — | C | — | — | — |
| Schema design / migration | C | — | — | — | R/A | — | C | — | — | — |
| Partition key design | C | — | — | — | — | R/A | C | — | — | — |
| Bug investigation | C | C | — | — | — | — | — | — | — | R/A |
| Data-layer code review | R | — | C | — | R* | R* | R | — | — | — |
| Frontend code review | — | R | — | — | — | — | R | — | — | — |
| Security audit | C | C | — | C | C | C | R/A | — | — | — |
| Data-heavy UI design | — | C | — | — | — | — | — | C | R/A | — |
| Architecture decision | C | C | R/A | C | C* | C* | C | — | — | — |
| Refactoring | R | R | C | — | — | — | C | — | — | — |

*\* = only when that persistence model is in use in the project*

---

## Project Profiles

Not every project needs all ten agents. Select the profile that matches your project type, then add agents as complexity grows. Code cleanliness rules apply to ALL profiles via `.claude/rules/` — they are not tied to a specific agent.

### Full-Stack Web Application
**Up to 10 agents.** This is the default profile for projects with a backend API, frontend SPA, and cloud deployment. Include data agents based on which persistence model(s) the project uses (SQL, Table Storage, or both).

### API-Only Service
**Up to 7 agents.** Drop React Architect, Modern UI Agent, and Data Clarifier. Include data agents based on persistence model.
- Backend Architect, Structural Architect, Infrastructure Architect
- SQL Data Architect and/or Table Storage Architect (based on persistence model)
- Sentinel, Debug Investigator

### Frontend-Only Application
**5 agents.** Drop Backend Architect, Structural Architect (unless frontend has its own architectural paradigm), Infrastructure Architect (unless deploying to cloud), and both data agents.
- React Architect, Modern UI Agent, Data Clarifier
- Sentinel, Debug Investigator

### Minimal (Small Scripts / Utilities)
**2 agents.** Core quality without ceremony.
- Sentinel, Debug Investigator

---

## Adaptation Guide

Run `/bootstrap-agents` in a target project to automatically set up the complete `.claude/` ecosystem. The bootstrap skill:

1. **Reads all templates** — agents, rules, skills, hooks, CLAUDE.md
2. **Auto-detects the project** — backend, frontend, infrastructure, data persistence, testing
3. **Asks 2-4 clarifying questions** — only what it can't detect
4. **Creates rules** — customized `.claude/rules/` files with project-specific file globs
5. **Creates hooks** — `.claude/settings.json` with PostToolUse prompt hook
6. **Creates agents** — customized `.claude/agents/` files with project context injected
7. **Creates skills** — `/debug-investigate`, `/clarify-data`, `/review-cleanliness`
8. **Creates memory, CLAUDE.md, reference docs** — with two-tier memory guidance
9. **Presents verification summary** — lists everything created with the four-layer architecture

The bootstrap skill is defined in `.claude/skills/bootstrap-agents/SKILL.md`.

### Three-Layer Review Architecture

The migration from a dedicated code-cleanliness agent to a rules-based system creates a layered enforcement architecture:

| Layer | Mechanism | When It Fires | What It Catches |
|-------|-----------|---------------|-----------------|
| **1. Rules** | `.claude/rules/*.md` with `globs:` frontmatter | Auto-loaded when matching files are in context | Domain-specific patterns (React anti-patterns, SQL anti-patterns, etc.) |
| **2. Hook** | PostToolUse prompt hook in `.claude/settings.json` | After every `Edit` or `Write` tool call | Violations of any loaded rules — immediate feedback |
| **3. Skills** | `/review-cleanliness` batch review | On demand (before commit, during review) | Comprehensive cleanliness sweep of staged changes |
| **4. Agents** | Specialized agents in `.claude/agents/` | During planning and design sessions | Architectural decisions, security audits, design guidance |

### Manual Setup

If you prefer manual setup over `/bootstrap-agents`, the template files are organized as:

```
.claude/
  agents/       — 10 agent templates with <!-- ADAPT --> markers
  rules/        — 9 rules templates (4 always-loaded, 5 with <!-- ADAPT --> globs)
  skills/       — 4 skill templates (bootstrap, debug-investigate, clarify-data, review-cleanliness)
  settings.json — PostToolUse hook template
CLAUDE.md       — Project CLAUDE.md template
docs/
  design-system.md  — Design system reference template
  architecture.md   — Architecture overview template
```

Each template file contains `<!-- ADAPT -->` markers where project-specific customization is needed. The core methodology (80-90% of each file) is ready to use as-is.

---

## Principles

1. **Every agent has one lens.** Agents don't try to be everything. Backend Architect doesn't comment on visual design. Sentinel doesn't comment on code style. Stay in your lane.

2. **Opinions over suggestions.** Agents are opinionated. They state what they recommend and why, not "you could do X or Y." Hedging wastes everyone's time.

3. **Evidence over intuition.** Findings cite specific code, specific rules, specific principles. "This feels wrong" is not a finding.

4. **Advisory, not blocking (mostly).** Rules flag violations but are advisory. Sentinel findings on critical vulnerabilities ARE blocking. The team knows the difference.

5. **Exit gracefully.** If an agent has nothing relevant to say about a change, it exits with no findings. No filler, no "everything looks good from my perspective" padding.

6. **Compound knowledge.** Agents with memory build institutional knowledge across sessions. Patterns discovered today inform reviews tomorrow.

7. **Self-maintaining.** Infrastructure Architect tracks deployment state. When agents discover their own definitions are stale, they recommend updates.

8. **Parallel by default.** Review team agents run in parallel. They don't wait for each other or build on each other's findings. Independent perspectives are the point.

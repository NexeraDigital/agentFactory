# Agent Factory: Upgraded for Modern Claude Code

This document explains how Agent Factory has been restructured to align with the latest Claude Code features, dramatically reducing token consumption while increasing the quality and coverage of automated code review.

## What Changed and Why

The original architecture relied on **always-on agent invocations** — every code change triggered 4-5 specialized agents to review it, each loading 7-15 KB of prompt context. A typical backend feature consumed ~200K tokens across planning and review cycles, with significant waste from agents that loaded, read the changed files, and exited with "no findings" because the change wasn't in their domain.

The upgraded architecture splits agent responsibilities into **rules** (automatic, cheap, always-on) and **agents** (on-demand, interactive, reasoning-heavy), leveraging three Claude Code features that didn't exist when the original architecture was designed.

## Two Features That Made This Possible

### 1. Auto-Loading Rules (`.claude/rules/`)

Claude Code now supports rules files in `.claude/rules/` with `paths:` YAML frontmatter for conditional loading:

```yaml
---
paths:
  - "**/*.cs"
  - "!**/*Test*.cs"
---
BE-001: No silent fallbacks...
```

When Claude opens a `.cs` file, backend rules automatically load into context. When Claude opens a `.tsx` file, React and UI rules load instead. Rules without `paths:` frontmatter (like `universal.md` and `security-universal.md`) load on every session.

**Impact**: The right rules are always in context for the file being edited — no agent invocation needed, no wasted tokens loading irrelevant domain rules.

### 2. Skills with Fork Context

Skills defined in `.claude/skills/` can run in isolated sub-contexts (`context: fork`), providing on-demand batch review without polluting the main conversation:

```
/review-cleanliness    — Batch code quality review
/security-review       — Run sentinel security audit
/debug-investigate     — Route to scientific debugging
/clarify-data          — Route to data-heavy UI specialist
```

**Impact**: Deep analysis is available on-demand without the cost of running it on every edit.

## The Three-Layer Architecture

The upgrade replaces always-on agent invocations with a layered review system where each layer catches different classes of issues at the appropriate cost:

```
Code is written
    │
    ▼
Layer 1: Auto-Loading Rules (.claude/rules/)
    Rules load automatically based on file type.
    Claude follows them DURING code generation.
    Most violations never get written.
    Cost: 0 extra tokens (loaded like CLAUDE.md)
    │
    ▼
Layer 2: Pre-Commit Hook
    Fires on every git commit.
    Groups staged files by domain, runs batch review.
    Catches cross-file issues that per-edit checks miss.
    Cost: ~30-50K tokens, once per commit
    │
    ▼
Layer 3: On-Demand Skills + Agents
    Skills for batch review commands.
    Agents for interactive planning and design sessions.
    Cost: Only when invoked
```

## What Happened to Each Agent

### Converted to Rules Only (1 agent)

| Agent | Outcome |
|-------|---------|
| **code-cleanliness** | Deleted as a standalone agent. All rules (method length, nesting depth, constructor params, declarative style) moved to `.claude/rules/code-cleanliness.md`. Purely rule-based checks don't need agent-level reasoning. |

### Kept As-Is (4 agents)

| Agent | Why |
|-------|-----|
| **azure-architect** | Multi-step workflows: Azure CLI, Bicep generation, deployment troubleshooting. Can't be reduced to rules. |
| **data-clarifier** | Entirely generative. Transforms "walls of data" into clear UI through interactive conversation. |
| **debug-investigator** | On-demand 7-phase scientific debugging. Requires Bash, WebSearch, deep causal reasoning. |
| **modern-ui-agent** | Active design sessions are generative. Anti-patterns and review checklist extracted to `.claude/rules/ui-design.md`; design system reference extracted to `docs/design-system.md`. |

### Slimmed to Planning Only — HYBRID (6 agents)

These agents keep their interactive planning and design capabilities but shed their review sections, which now live in auto-loading rules:

| Agent | Review Rules Extracted To | Planning Role Retained |
|-------|--------------------------|----------------------|
| **backend-architect** | `.claude/rules/backend.md` (BE-001–BE-008) | Interface design, GoF patterns, SOLID architecture |
| **react-architect** | `.claude/rules/react.md` (RC-001–RC-008) | Component hierarchy, state management strategy |
| **sentinel** | `.claude/rules/security-universal.md` (SEC-001–SEC-006) | Full 8-step security audit (kept intact) |
| **idesign-architect** | `.claude/rules/idesign.md` (ID-001–ID-011) | Thin orchestrator reading IDesign reference docs on demand |
| **sql-data-architect** | `.claude/rules/sql.md` (SQL-001–SQL-007) | Schema design, migration planning, query optimization |
| **table-storage-architect** | `.claude/rules/table-storage.md` (TS-001–TS-006) | Partition key design, denormalization strategy |

## Token Savings

The upgrade delivers significant token savings by eliminating the three biggest cost drivers:

| Cost Driver | Before | After |
|-------------|--------|-------|
| Post-completion review (5 agents) | ~100K tokens | ~3-5K (auto-loaded rules in context) |
| "No findings" exits (irrelevant agents) | ~20-30K tokens | 0 (rules don't load for irrelevant domains) |
| Opus for simple pattern matching | ~5x cost premium | Rules checked at session model level |
| Agent prompt loading | 7-15 KB per agent per invocation | Rules: ~2-3 KB per domain, only matching |

**Overall: ~200K tokens/feature reduced to ~55K tokens/feature (~72% savings)**

Planning sessions (when you explicitly invoke an agent for design work) remain unchanged — you pay for reasoning when you need reasoning. The savings come entirely from eliminating expensive agent invocations for work that auto-loading rules handle better.

## CLAUDE.md: Lean Index Pattern

Instead of inlining hundreds of lines of rules into CLAUDE.md (which loads on every conversation), it now serves as a lean index:

- **Universal rules** (5-10 lines) — the highest-value rules that apply to every edit
- **Pointers to reference docs** — `@docs/architecture.md`, `@docs/design-system.md`
- **Agent dispatch table** — which agent to invoke for which type of work

Domain-specific rules auto-load from `.claude/rules/` based on file type. A developer fixing a CSS bug loads only UI and cleanliness rules — not SQL rules, not IDesign rules.

## Rules Coverage

The upgrade produced 9 rules files with 56 individually-identified rules across all domains:

| Rules File | Rule IDs | Scope |
|------------|----------|-------|
| `universal.md` | UNI-001–UNI-005 | All code, always loaded |
| `security-universal.md` | SEC-001–SEC-006 | All code, always loaded |
| `backend.md` | BE-001–BE-008 | `**/*.cs` |
| `react.md` | RC-001–RC-008 | `**/*.tsx`, `**/*.ts` |
| `idesign.md` | ID-001–ID-011 | IDesign layer files |
| `sql.md` | SQL-001–SQL-007 | Accessor files, migrations |
| `table-storage.md` | TS-001–TS-006 | Table Storage files |
| `code-cleanliness.md` | CC-001–CC-003 | All source files |
| `ui-design.md` | (checklist-based) | Frontend files |

Every rule has a severity level (Critical/High/Medium/Low) and includes concrete bad/good code examples so Claude can self-review against them.

## The Core Principle

> **Rules handle the "what to check" automatically; agents handle the "how to think" on demand.**

Binary rule checking (forbidden patterns, numeric thresholds) doesn't need a dedicated agent — it needs a rules file that's always in context. Interactive design reasoning (architecture decisions, schema planning, security audits) does need an agent — but only when you ask for it, not on every edit.

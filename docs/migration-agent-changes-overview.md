# Agent Changes Overview

This document summarizes what happens to each of the 11 agents and explains the new architecture that replaces always-on agent invocations.

## The Big Picture

Today, every code change triggers 4-5 agents to review it. Each agent loads its full prompt (7-15 KB), reads the changed files, and produces a review -- even when the change isn't relevant to that agent's domain. This costs ~200K tokens per feature and creates significant latency.

The new architecture splits each agent's work into two roles:

- **Review role** (rule checking): Moves to auto-loading rules + lightweight hooks. Automatic, cheap, always-on.
- **Planning role** (design, architecture): Stays as a slimmed-down agent. Invoked on-demand when you need interactive reasoning.

**Result: ~72% token savings per feature, with nearly all review capabilities preserved.**

---

## What Happens to Each Agent

### Deleted (1 agent)

| Agent | What Happens | Why |
|-------|-------------|-----|
| **code-cleanliness** | Deleted entirely. Rules move to `.claude/rules/code-cleanliness.md`. | Purely rule-based (method length, nesting, constructor params). No planning role. Every rule is a concrete threshold or pattern -- exactly what auto-loading rules handle best. |

### Kept As-Is (4 agents)

| Agent | What Happens | Why |
|-------|-------------|-----|
| **azure-architect** | No changes. | Multi-step workflows: Azure CLI, Bicep generation, self-maintenance. Can't be reduced to rules. |
| **data-clarifier** | No changes. | Entirely generative and on-demand. Clarifies ambiguous data requirements through conversation. |
| **debug-investigator** | No changes. | On-demand 7-phase debugging workflow. Requires Bash, WebSearch. Reacts to bugs, not edits. |
| **modern-ui-agent** | Kept for design sessions, but anti-patterns and review checklist extracted to `.claude/rules/ui-design.md`. Design system reference extracted to `docs/design-system.md`. | Primary value is active design sessions (creating new designs). The review checklist is rule-based and moves to auto-loading rules. |

### Slimmed Down -- HYBRID (6 agents)

These agents keep their **planning** role but lose their **review** sections. The review rules move to `.claude/rules/` where they auto-load and are enforced automatically.

| Agent | What's Removed | What's Kept | Rules File Created |
|-------|---------------|-------------|-------------------|
| **backend-architect** | "Red Flag Patterns" section, "Self-Verification" checklist | Planning methodology, SOLID principles, GoF patterns, "When Making Changes" | `.claude/rules/backend.md` (BE-001 to BE-008) |
| **react-architect** | "Anti-Patterns to Flag" section | Component hierarchy design, state management strategy, methodology | `.claude/rules/react.md` (RC-001 to RC-008) |
| **sentinel** | Nothing removed from agent | Full 8-step audit methodology kept intact | `.claude/rules/security-universal.md` (SEC-001 to SEC-006) |
| **idesign-architect** | "Violation Detection Methodology" section | Layer definitions, planning guidance, feedback format (becomes thin orchestrator reading `docs/idesign-reference.md` and `docs/idesign-implementation.md`) | `.claude/rules/idesign.md` (ID-001 to ID-007) |
| **sql-data-architect** | Anti-patterns table | Schema design philosophy, migration strategy, indexing guidance | `.claude/rules/sql.md` (SQL-001 to SQL-007) |
| **table-storage-architect** | Anti-patterns table | Partition key design philosophy, mental model shift table, design principles | `.claude/rules/table-storage.md` (TS-001 to TS-006) |

**Note on sentinel**: The agent is kept fully intact because its 8-step audit is a deep, multi-file workflow that can't be rule-based. The 6 "Scary Patterns" are *also* extracted to rules for automatic per-edit checking -- the hook supplements the agent, it doesn't replace it.

---

## The New Review Architecture

Instead of agents reviewing after every change, a four-layer architecture handles review at different points in the workflow:

```
Developer writes code
        |
        v
 [Layer 1: Auto-Loading Rules]
 .claude/rules/*.md files load automatically
 based on which files Claude is working with.
 Claude follows rules DURING code generation.
 Most violations never get written.
 Cost: 0 extra tokens (loaded like CLAUDE.md).
        |
        v
 [Layer 2: PostToolUse Hooks]
 After each Edit/Write, a prompt hook asks Claude
 to verify the edit against rules already in context.
 Catches anything that slipped through Layer 1.
 Cost: Low (~2-5K tokens per triggered check).
        |
        v
 Developer runs `git commit`
        |
        v
 [Layer 3: Husky Pre-Commit Hook]
 .husky/pre-commit fires on every git commit.
 Groups staged files by domain, runs batch review.
 Can invoke Claude Agent SDK for AI-powered
 cross-file analysis, or run static linters.
 Catches cross-file issues that per-edit hooks miss.
 Cost: Medium (~30-50K tokens, once per commit).
        |
        v
 [Layer 4: On-Demand Skills + Agents]
 .claude/skills/ for batch review commands
   (/review-backend, /review-security, etc.)
 .claude/agents/ for HYBRID agents in planning mode
 Invoked manually when deep analysis is needed.
 Cost: Only when invoked.
```

### How Rules Auto-Load

Rules files in `.claude/rules/` use `paths:` YAML frontmatter to control when they load:

```yaml
# .claude/rules/backend.md
---
paths:
  - "**/*.cs"
  - "!**/*Test*.cs"
---
BE-001: No silent fallbacks...
BE-002: No swallowed exceptions...
```

When Claude opens a `.cs` file, backend rules automatically load into context. When Claude opens a `.tsx` file, React and UI rules load instead. Security rules (no `paths:` frontmatter) load every session.

This eliminates the need for bash scripts that check file extensions -- the right rules are always in context for the file being edited.

### How Hooks Work

Hook configuration lives in `.claude/settings.json` using `prompt` or `agent` handler types:

```json
{
  "hooks": {
    "PostToolUse": [
      {
        "matcher": "Edit|Write",
        "hooks": [
          {
            "type": "prompt",
            "prompt": "Check if the edit just made violates any rules loaded in context. If violations found, list each with [RULE-ID] [SEVERITY] description."
          }
        ]
      }
    ]
  }
}
```

No bash scripts needed. The `prompt` type triggers a lightweight LLM check. The `agent` type can do multi-file tracing when cross-file analysis is needed.

---

## What This Means in Practice

### For day-to-day coding with Claude Code

- Rules are always active -- Claude follows them while writing code (Layer 1)
- Every edit gets a quick automated check (Layer 2)
- Every commit gets a thorough cross-file review (Layer 3)
- You can still invoke any HYBRID agent for planning: "use the backend-architect to design this feature"

### For planning and design sessions

- HYBRID agents still exist for interactive design work
- They're just leaner -- review checklists removed since hooks handle that now
- idesign-architect reads the IDesign reference docs on demand instead of carrying 683 lines in its prompt

### For deep audits

- Sentinel's full 8-step security audit is unchanged
- Skills like `/review-backend` and `/review-security` provide on-demand batch reviews
- These run in isolated subagents (`context: fork`) so they don't pollute the main session

### Token savings

| Scenario | Before (Agents) | After (New Architecture) | Savings |
|----------|-----------------|-------------------------|---------|
| Post-completion review (5 agents) | ~100K tokens | ~3-5K (auto-loaded rules) | ~95% |
| "No findings" exits | ~20-30K tokens | 0 (rules don't load for irrelevant domains) | 100% |
| Planning (agents invoked) | ~80K tokens | ~80K tokens | 0% |
| **Total per feature** | **~200K** | **~55K** | **~72%** |

---

## File Structure After Migration

```
.claude/
├── rules/                              # Auto-loading rules (Layer 1)
│   ├── universal.md                    # Always loaded -- method length, nesting, constructors
│   ├── security-universal.md           # Always loaded -- user scoping, auth
│   ├── backend.md                      # Loads for **/*.cs
│   ├── react.md                        # Loads for **/*.tsx, **/*.ts
│   ├── idesign.md                      # Loads for **/*Manager.cs, *Engine.cs, *Accessor.cs
│   ├── sql.md                          # Loads for **/*Accessor.cs, **/Migrations/**
│   ├── table-storage.md                # Loads for **/TableStorage/**
│   ├── code-cleanliness.md             # Loads for **/*.cs, **/*.tsx, **/*.ts
│   └── ui-design.md                    # Loads for **/*.tsx, **/*.css, **/*.scss
├── settings.json                       # Hook configuration (Layer 2)
├── skills/                             # On-demand batch review (Layer 4)
│   ├── review-backend/SKILL.md
│   ├── review-security/SKILL.md
│   └── review-cleanliness/SKILL.md
└── agents/                             # Slimmed HYBRID agents (Layer 4)
    ├── backend-architect.md            # Planning only
    ├── react-architect.md              # Planning only
    ├── sentinel.md                     # Full agent (kept intact)
    ├── idesign-architect.md            # Thin orchestrator
    ├── sql-data-architect.md           # Planning only
    ├── table-storage-architect.md      # Planning only
    ├── modern-ui-agent.md              # Design sessions only
    ├── azure-architect.md              # Unchanged
    ├── data-clarifier.md               # Unchanged
    └── debug-investigator.md           # Unchanged

.husky/
└── pre-commit                          # Batch cross-file review (Layer 3)

docs/
├── design-system.md                    # Extracted from modern-ui-agent
├── architecture.md                     # IDesign layers + project structure
├── idesign-reference.md                # IDesign methodology (read by idesign-architect)
└── idesign-implementation.md           # IDesign coding patterns (read by idesign-architect)
```

---

## Summary

| Category | Count | Agents |
|----------|-------|--------|
| **CONVERT** (deleted, replaced by rules) | 1 | code-cleanliness |
| **KEEP** (unchanged or minimally changed) | 4 | azure-architect, data-clarifier, debug-investigator, modern-ui-agent |
| **HYBRID** (slimmed, review role → rules) | 6 | backend-architect, react-architect, sentinel, idesign-architect, sql-data-architect, table-storage-architect |

The core idea: **rules handle the "what to check" automatically; agents handle the "how to think" on demand.** Binary rule checking (forbidden patterns, thresholds) doesn't need a dedicated agent -- it needs a rules file that's always in context. Interactive design reasoning (architecture decisions, schema planning) does need an agent -- but only when you ask for it, not on every edit.

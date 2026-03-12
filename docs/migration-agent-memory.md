# Agent Memory Analysis

## Current Memory Design

Five of 11 agents have `memory: project` in their YAML frontmatter. The intended mechanism: each agent gets a persistent file at `.claude/agent-memory/<agent-name>/MEMORY.md` that is loaded into the agent's system prompt on every invocation. Agents self-curate this file as they discover project-specific patterns across sessions.

| Agent | Memory Enabled | What They're Told to Store |
|-------|---------------|---------------------------|
| code-cleanliness | Yes | Code patterns, recurring violations, style conventions, method length trends, coupling patterns |
| react-architect | Yes | Component patterns, hook conventions, state management approaches, routing patterns, performance optimizations |
| sql-data-architect | Yes | Schema patterns, indexing decisions, migration strategies, query performance findings |
| table-storage-architect | Yes | Partition strategies, entity patterns, query patterns, denormalization decisions |
| data-clarifier | Yes | UI patterns, data density problems, component decisions, layout preferences |
| backend-architect | No | — |
| sentinel | No | — |
| idesign-architect | No | — |
| azure-architect | No | — |
| debug-investigator | No | — |
| modern-ui-agent | No | — |

All five memory-enabled agents share identical memory instructions:

```
You have a persistent agent memory directory. Its contents persist across conversations.

As you work, consult your memory files to build on previous experience.

Guidelines:
- MEMORY.md is always loaded into your system prompt — lines after 200 will be truncated
- Create separate topic files for detailed notes and link to them from MEMORY.md
- Update or remove memories that turn out to be wrong or outdated
- Organize memory semantically by topic, not chronologically
```

Each agent also has a domain-specific prompt like: *"Update your agent memory as you discover [domain-specific patterns] across the project. This builds institutional knowledge across conversations."*

The bootstrap procedure (`bootstrap-agents.md`, Phase 5) creates these directories and empty `MEMORY.md` files during project setup. Currently, no `.claude/agent-memory/` directory exists in this template repo -- memory is populated only after bootstrap into a target project.

## How Memory Is Supposed to Compound

The methodology (`agent-architecture.md`, Principle 6) states:

> *"Compound knowledge. Agents with memory build institutional knowledge across sessions. Patterns discovered today inform reviews tomorrow."*

The intended lifecycle:
1. Agent is invoked → loads `MEMORY.md` into system prompt
2. Agent reviews code → discovers a pattern (e.g., "reporting module tends to have 25+ line methods")
3. Agent writes the discovery to `MEMORY.md`
4. Next session → agent loads updated memory → knows to pay extra attention to the reporting module
5. Over time, reviews become more targeted and efficient

## Problems with the Current Memory Design

### Problem 1: Knowledge Is Siloed by Agent

Code-cleanliness discovers "the reporting module has long methods." React-architect discovers "the reporting dashboard uses prop drilling." SQL-data-architect discovers "the reporting queries have N+1 issues." These are all about the same module but stored in three separate memory files that never cross-reference.

No agent sees the full picture. No mechanism promotes cross-agent knowledge. The main Claude session doesn't see any of it.

### Problem 2: ~70% of Stored Knowledge Should Be in CLAUDE.md

The agents are instructed to store things like "codebase style conventions," "component patterns," and "schema patterns." These are project-level facts that belong in CLAUDE.md where all agents, all hooks, and the main session can benefit -- not siloed in one agent's memory.

| Knowledge Type | Currently Stored In | Should Be In |
|---------------|-------------------|-------------|
| Project-wide code style conventions | code-cleanliness memory | CLAUDE.md |
| Component architecture patterns | react-architect memory | CLAUDE.md |
| Hook naming conventions | react-architect memory | CLAUDE.md |
| Schema design decisions | sql-data-architect memory | CLAUDE.md |
| Partition key strategies | table-storage-architect memory | CLAUDE.md |
| UI layout preferences | data-clarifier memory | CLAUDE.md |
| Recurring violation hotspots | code-cleanliness memory | Agent memory (correct placement) |
| Query performance findings | sql-data-architect memory | Agent memory (correct placement) |
| Module-specific problem areas | any memory-enabled agent | Agent memory (correct placement) |

Only agent-specific observations ("this module tends to violate this rule") truly belong in agent-scoped memory. Project-wide facts belong where everything can see them.

### Problem 3: Memory Adds Tokens on Every Invocation, Including Wasted Exits

Each memory-enabled agent loads its full `MEMORY.md` (up to 200 lines) on every invocation. When code-cleanliness is invoked for a review and exits with "no findings" because only infrastructure files changed, it still loaded its memory. As memory accumulates toward the 200-line limit, this becomes a meaningful token cost for zero-value invocations.

With 5 memory-enabled agents on the review team, a single review cycle could load up to 1,000 lines of memory content (~5 x 200 lines) before any code is even read. On "no findings" exits, every one of those tokens is wasted.

### Problem 4: No Quality Control on Stored Knowledge

Agents self-curate with the instruction "update or remove memories that turn out to be wrong or outdated." But there's no verification mechanism. An agent could store an incorrect observation (e.g., "this project follows repository pattern" when it actually uses IDesign accessors) that persists across sessions and subtly misguides future reviews.

Unlike CLAUDE.md, which is version-controlled and human-reviewed, agent memory files accumulate silently. Nobody reviews what agents write to their memory unless they go looking.

### Problem 5: Memory Is Orphaned by Migration

When code-cleanliness is deleted (CONVERT verdict), its accumulated memory about recurring violations in specific modules is lost. The rules+hooks replacement has no memory mechanism. Knowledge that took 20 sessions to accumulate disappears.

For HYBRID agents that are slimmed down, the memory survives (the agent still exists for planning), but the review-specific observations ("this module always fails CC-002") are no longer relevant since the agent no longer does reviews.

## Recommendation: Two-Tier Memory Model

Instead of agent-scoped memory that siloes knowledge, restructure into two tiers:

### Tier 1: Project Knowledge → CLAUDE.md

Visible to all agents, all hooks, and the main session. Contains:
- Code conventions and style patterns
- Component architecture patterns and hook conventions
- Schema design decisions and indexing strategies
- Partition key strategies and data model decisions
- UI layout preferences and design system choices
- Any knowledge that multiple agents or the main session would benefit from

This is the "institutional knowledge" that the methodology promises -- but placed where it actually compounds across the entire system, not just within one agent.

### Tier 2: Agent-Specific Observations → Agent Memory

Visible to one agent only. Contains only things that are genuinely agent-specific:
- Recurring violation hotspots ("reporting module tends to violate CC-001")
- Performance regression warnings ("OrderAccessor N+1 was fixed in PR #42, watch for regression")
- Module-specific problem patterns ("the auth middleware has complex branching that resists cleanup")

This keeps memory files small (well under 200 lines), reduces wasted tokens on irrelevant invocations, and ensures the most valuable knowledge is in CLAUDE.md where it reaches everything.

### Impact on Migration

| Agent | Verdict | Memory Action |
|-------|---------|--------------|
| code-cleanliness | CONVERT (delete) | Migrate project knowledge to CLAUDE.md. Violation hotspot knowledge → project-specific notes in rules file. Memory directory deleted. |
| react-architect | HYBRID | Keep memory for planning. Move project-wide patterns (component conventions) to CLAUDE.md. Memory file becomes smaller, agent-specific only. |
| sql-data-architect | HYBRID | Keep memory for planning. Move schema decisions to CLAUDE.md. Memory retains only performance findings and regression warnings. |
| table-storage-architect | HYBRID | Keep memory for planning. Move partition strategies to CLAUDE.md. Memory retains only query pattern observations. |
| data-clarifier | KEEP | No changes. Agent and memory stay as-is. |

### What Changes in the Bootstrap

`bootstrap-agents.md` Phase 5 should be updated:
- Add guidance distinguishing Tier 1 (CLAUDE.md) vs. Tier 2 (agent memory) knowledge
- Instruct agents to write project-wide discoveries to CLAUDE.md (or flag them for promotion)
- Agent memory instructions should say: *"Store only agent-specific observations here. Project-wide conventions and patterns belong in CLAUDE.md."*

`agent-architecture.md` Step 5 and Principle 6 should be updated:
- Describe the two-tier model
- Clarify that "institutional knowledge" compounds in CLAUDE.md, not just in agent memory
- Update the memory instruction template in agent definitions

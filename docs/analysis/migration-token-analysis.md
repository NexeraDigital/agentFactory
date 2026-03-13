# Token Cost Analysis

The current agent architecture has structural properties that compound token usage. This section quantifies the problem and shows the savings the hook+rules pattern provides.

## Problem 1: Every Change Triggers 4-5 Agent Invocations

The methodology mandates Tier 1 agents (backend-architect, react-architect, sentinel, code-cleanliness) on the post-completion review team for **every** change. Several Tier 2 agents also participate when their domain is relevant.

A typical backend feature touching SQL might invoke agents at all three workflow stages:

| Stage | Agents Invoked | Model | Notes |
|-------|---------------|-------|-------|
| Planning | backend-architect | opus | Designs approach |
| Planning | idesign-architect | opus | Validates layer placement |
| Planning | sentinel | opus | Identifies security concerns |
| Planning | sql-data-architect | opus | Designs schema |
| Authoring | sql-data-architect | opus | Guides data layer work |
| Review | backend-architect | opus | Validates code quality |
| Review | idesign-architect | opus | Checks forbidden dependencies |
| Review | sentinel | opus | Security audit |
| Review | code-cleanliness | sonnet | Method length, nesting, coupling |
| Review | react-architect | opus | **Exits immediately -- no frontend changes** |

**10 agent invocations for a single feature, 9 on opus.**

A frontend-only change still triggers backend-architect, sentinel, and code-cleanliness at review -- 3 of which will determine there's nothing backend-relevant and exit. You pay full context load for a "no findings" response.

## Problem 2: Agent Prompt Sizes Are Large

Each agent invocation loads the full agent definition into context as input tokens. These prompts are substantial:

| Agent | Lines | Approx. Size | Model |
|-------|-------|-------------|-------|
| modern-ui-agent | 337 | ~15 KB | opus |
| table-storage-architect | 228 | ~14 KB | opus |
| sql-data-architect | 196 | ~13 KB | opus |
| debug-investigator | 217 | ~9 KB | opus |
| sentinel | 171 | ~8 KB | opus |
| backend-architect | 161 | ~7.5 KB | opus |
| react-architect | 144 | ~8 KB | opus |
| code-cleanliness | 125 | ~7.2 KB | sonnet |
| data-clarifier | 114 | ~7 KB | opus |
| idesign-architect | 153 | ~6.7 KB | opus |
| azure-architect | 176 | ~7.6 KB | sonnet |

Five Tier 1 agents loading for a post-completion review consume ~46 KB of prompt context before any code is read. The agents then read the changed files, adding more input tokens. The total input per review round can easily reach 60-80 KB.

## Problem 3: "No Findings" Isn't Free

Agents that "exit with no findings" when changes aren't in their domain still consume tokens:

1. **Input tokens**: Full agent prompt loaded (~7-15 KB each)
2. **Input tokens**: Changed files read by the agent to determine relevance
3. **Output tokens**: The agent's "no findings" response
4. **Latency**: Each agent invocation adds wall-clock time

For a backend-only change, react-architect loads its 144-line prompt, reads the changed files, determines they're all `.cs` files, and outputs "No frontend changes detected." That's ~10-15K input tokens and several seconds of latency for zero value.

## Problem 4: Opus for Simple Rule Checks

8 of 11 agents specify `model: opus`. Some of these run rule checks that don't require opus-level reasoning:

- **code-cleanliness**: Counting method lines and checking for nested ifs -- sonnet (or even haiku) is sufficient
- **idesign-architect** review mode: Checking if a Manager file imports another Manager -- a pattern match, not deep reasoning
- **sentinel** for the 6 Scary Patterns: Checking whether a query includes a user scoping predicate -- concrete pattern matching

Using opus for these checks costs ~5x more per token than sonnet and ~15x more than haiku.

## Problem 5: Memory-Enabled Agents Add More Context

Five agents have `memory: project` enabled (react-architect, code-cleanliness, data-clarifier, sql-data-architect, table-storage-architect). Each invocation loads MEMORY.md and linked memory files, adding input tokens to every call even when the agent exits with no findings.

## Estimated Token Cost Per Feature

**Current architecture (single backend+SQL feature):**

| Component | Input Tokens (est.) | Output Tokens (est.) | Model |
|-----------|-------------------|---------------------|-------|
| 4x planning agents | ~80K | ~8K | opus |
| 1x authoring agent | ~20K | ~4K | opus |
| 5x review agents | ~100K | ~10K | mostly opus |
| **Total** | **~200K input** | **~22K output** | |

At opus pricing, this is a meaningful cost for every feature -- and it scales linearly with the number of edit-review cycles during development.

## How Hook+Rules Reduces Token Cost

The hook+rules pattern eliminates the biggest cost drivers:

| Cost Driver | Current (Agents) | After (Hook+Rules) |
|-------------|------------------|---------------------|
| Irrelevant agent invocations | 1-2 agents "exit with no findings" per review (~20-30K input tokens wasted) | Rules only load for relevant file types via `paths:` frontmatter -- 0 tokens for irrelevant domains |
| Agent prompt loading | 7-15 KB per agent, per invocation | Rules auto-load into context (~2-3 KB per domain, only matching domains) |
| Model selection | 8 of 11 agents force opus | `prompt` hooks use Haiku-level calls; main session uses whatever model is active |
| Review trigger | Manual invocation or workflow convention | Automatic via PostToolUse `prompt` hooks |
| Memory loading | Loaded on every agent invocation | Not applicable -- rules are static files in `.claude/rules/` |
| Multi-stage duplication | Same agent loads at planning AND review | `prompt`/`agent` hooks handle review; agent only invoked for planning |
| Pre-commit review | N/A | Husky pre-commit hook -- separate Agent SDK invocation, not charged to Claude Code session |

### Three-Layer Cost Model

The revised architecture distributes cost across three distinct layers, each with different pricing characteristics:

- **Layer 1 (`.claude/rules/`)**: Rules auto-load into context. Universal rules (no `paths:` frontmatter) load every session (~1-2K tokens). Domain rules (with `paths:`) load conditionally (~2-3K per domain, only when matching files are accessed). Cost is similar to CLAUDE.md -- loaded once per session, not per edit.
- **Layer 2 (PostToolUse `prompt` hooks)**: Each triggered edit incurs a Haiku-level LLM call. At ~$0.25 per million input tokens (vs ~$15 for Opus), this is cheap per-trigger. Replaces expensive Opus agent invocations for inline review.
- **Layer 3 (Husky pre-commit)**: Runs entirely outside the Claude Code session as a separate process. If using the Agent SDK for AI review, it is a separate API call not charged to the session. If using static linters, it is free. Runs once per commit rather than per edit.

**Estimated savings per feature:**

| Scenario | Current | After | Savings |
|----------|---------|-------|---------|
| Post-completion review (5 agents) | ~100K input tokens | ~3-5K (auto-loaded rules in context) | **~95%** |
| "No findings" exits (1-2 agents) | ~20-30K input tokens | 0 tokens (rules don't load for irrelevant domains) | **100%** |
| Per-edit prompt hooks | N/A | ~1-2K per trigger (Haiku-level) | New cost, but cheap |
| Husky pre-commit review | N/A | Separate Agent SDK cost (not session tokens) | External cost |
| Planning (agents still invoked) | ~80K input tokens | ~80K input tokens | 0% (unchanged) |
| **Total per feature** | **~200K** | **~85K** | **~57%** |

The review phase sees the largest savings because it is the most frequent (runs after every change, not just once per feature) and has the most wasted invocations. Over a development session with 10-20 edit-review cycles, the cumulative savings are substantial.

## When the Agent Is Still Worth the Tokens

The KEEP and HYBRID planning roles are worth their token cost because they provide value that can't be replicated by simple rules:

- **Sentinel's full 8-step audit**: Multi-file data flow tracing finds vulnerabilities that per-file rules miss
- **Backend/React architect planning**: Interactive design sessions that produce component trees and interface designs
- **Debug investigator**: 7-phase root cause analysis requires deep reasoning
- **Azure architect**: Infrastructure workflows with CLI execution

The principle: **pay for reasoning when you need reasoning; don't pay for pattern matching.**

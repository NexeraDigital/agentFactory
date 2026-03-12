# Hook+Rules Migration Guide

This document describes how to evolve agentFactory's 11 agent definitions to take advantage of Claude Code's PostToolUse hook system. The hook+rules pattern provides automatic, edit-triggered review without requiring manual agent invocation.

## The Hook+Rules Pattern

Instead of invoking a standalone agent after every code change, this pattern:

1. **Rules file** (`docs/review-rules/<domain>-rules.md`) -- Documented rules with IDs, severity levels, and bad/good code examples. Claude reads this file and self-reviews against it.
2. **PostToolUse hook** (`.claude/hooks/`) -- A tiny script that reads stdin JSON from Claude Code, checks if the edited file is relevant, and exits with code 2 to feed a review prompt back via stderr.
3. **CLAUDE.md entries** -- The highest-value rules inlined so Claude follows them proactively during generation (before the hook even fires).
4. **Slash command** (optional) -- On-demand batch review via `git diff` for comprehensive sweeps.

The hook does NO analysis. It triggers Claude's self-review against the rules file. Claude IS the reviewer.

---

## Token Cost Analysis

The current agent architecture has structural properties that compound token usage. This section quantifies the problem and shows the savings the hook+rules pattern provides.

### Problem 1: Every Change Triggers 4-5 Agent Invocations

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

### Problem 2: Agent Prompt Sizes Are Large

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

### Problem 3: "No Findings" Isn't Free

Agents that "exit with no findings" when changes aren't in their domain still consume tokens:

1. **Input tokens**: Full agent prompt loaded (~7-15 KB each)
2. **Input tokens**: Changed files read by the agent to determine relevance
3. **Output tokens**: The agent's "no findings" response
4. **Latency**: Each agent invocation adds wall-clock time

For a backend-only change, react-architect loads its 144-line prompt, reads the changed files, determines they're all `.cs` files, and outputs "No frontend changes detected." That's ~10-15K input tokens and several seconds of latency for zero value.

### Problem 4: Opus for Simple Rule Checks

8 of 11 agents specify `model: opus`. Some of these run rule checks that don't require opus-level reasoning:

- **code-cleanliness**: Counting method lines and checking for nested ifs -- sonnet (or even haiku) is sufficient
- **idesign-architect** review mode: Checking if a Manager file imports another Manager -- a pattern match, not deep reasoning
- **sentinel** for the 6 Scary Patterns: Checking whether a query includes a user scoping predicate -- concrete pattern matching

Using opus for these checks costs ~5x more per token than sonnet and ~15x more than haiku.

### Problem 5: Memory-Enabled Agents Add More Context

Five agents have `memory: project` enabled (react-architect, code-cleanliness, data-clarifier, sql-data-architect, table-storage-architect). Each invocation loads MEMORY.md and linked memory files, adding input tokens to every call even when the agent exits with no findings.

### Estimated Token Cost Per Feature

**Current architecture (single backend+SQL feature):**

| Component | Input Tokens (est.) | Output Tokens (est.) | Model |
|-----------|-------------------|---------------------|-------|
| 4x planning agents | ~80K | ~8K | opus |
| 1x authoring agent | ~20K | ~4K | opus |
| 5x review agents | ~100K | ~10K | mostly opus |
| **Total** | **~200K input** | **~22K output** | |

At opus pricing, this is a meaningful cost for every feature -- and it scales linearly with the number of edit-review cycles during development.

### How Hook+Rules Reduces Token Cost

The hook+rules pattern eliminates the biggest cost drivers:

| Cost Driver | Current (Agents) | After (Hooks+Rules) |
|-------------|------------------|---------------------|
| Irrelevant agent invocations | 1-2 agents "exit with no findings" per review (~20-30K input tokens wasted) | Hook checks file path in bash -- 0 tokens if not relevant |
| Agent prompt loading | 7-15 KB per agent, per invocation | Rules are in CLAUDE.md (loaded once) or read on-demand from a ~2-3 KB rules file |
| Model selection | 8 of 11 agents force opus | Self-review runs on whatever model the session is already using |
| Review trigger | Manual invocation or workflow convention | Automatic on every edit -- no extra invocation needed |
| Memory loading | Loaded on every agent invocation | Not applicable -- rules are static files |
| Multi-stage duplication | Same agent loads at planning AND review | Hook handles review; agent only invoked for planning when needed |

**Estimated savings per feature:**

| Scenario | Current | After | Savings |
|----------|---------|-------|---------|
| Post-completion review (5 agents) | ~100K input tokens | ~5K (rules file read) | **~95%** |
| "No findings" exits (1-2 agents) | ~20-30K input tokens | 0 tokens | **100%** |
| Planning (agents still invoked) | ~80K input tokens | ~80K input tokens | 0% (unchanged) |
| **Total per feature** | **~200K** | **~85K** | **~57%** |

The review phase sees the largest savings because it's the most frequent (runs after every change, not just once per feature) and has the most wasted invocations. Over a development session with 10-20 edit-review cycles, the cumulative savings are substantial.

### When the Agent Is Still Worth the Tokens

The KEEP and HYBRID planning roles are worth their token cost because they provide value that can't be replicated by simple rules:

- **Sentinel's full 8-step audit**: Multi-file data flow tracing finds vulnerabilities that per-file rules miss
- **Backend/React architect planning**: Interactive design sessions that produce component trees and interface designs
- **Debug investigator**: 7-phase root cause analysis requires deep reasoning
- **Azure architect**: Infrastructure workflows with CLI execution

The principle: **pay for reasoning when you need reasoning; don't pay for pattern matching.**

---

## Functionality Risk Assessment

The hook+rules pattern is not a free swap. Several capabilities that agents provide are reduced or lost entirely. This section catalogs each risk, rates its severity, and proposes mitigations.

### Risk 1: Loss of Cross-File Analysis

**Severity: High**

Agents can read multiple files and trace relationships. Sentinel traces data flow from controller through service to data access layer. IDesign architect greps across all Manager/Engine/Accessor files to find forbidden dependencies. A PostToolUse hook fires on a **single file edit** -- it sees only the file that was just changed, not how it connects to the rest of the codebase.

**What's lost**: A hook reviewing `OrderManager.cs` won't automatically read `OrderAccessor.cs` to verify user scoping reaches the data layer. An agent would.

**Mitigation**: The hook's stderr prompt can instruct Claude to read related files: *"Review this edit and trace any data access calls to their accessor implementations to verify user scoping."* Claude in the main session has full tool access and can follow the trail. However, this adds tokens back and depends on Claude choosing to do the tracing -- it's not guaranteed the way an agent's explicit workflow is.

**Residual risk**: Medium. The hook can prompt for cross-file review but can't enforce the systematic multi-file methodology the way a dedicated agent workflow does.

### Risk 2: Loss of Persistent Agent Memory

**Severity: Medium**

Five agents have `memory: project` -- they accumulate institutional knowledge across sessions: recurring violations, codebase conventions, method length trends, coupling patterns. This memory makes reviews more targeted over time. Hooks have no memory mechanism.

**What's lost**: An agent that has reviewed 20 sessions worth of code knows "this codebase tends to violate CC-002 in the reporting module" and watches for it. A hook treats every review as the first time.

**Mitigation**: The highest-value institutional knowledge can be captured in CLAUDE.md or the rules files as project-specific notes. The main conversation's memory system (`.claude/projects/.../memory/`) can also store patterns. But neither is a direct replacement for agent-scoped memory that builds automatically.

**Residual risk**: Low-Medium. Most agent memory content is derivable from the code itself. The agents that benefit most from memory (sql-data-architect, table-storage-architect) are HYBRID -- they keep their agents for planning, where memory is most valuable.

### Risk 3: Loss of Specialized Persona and Expertise Framing

**Severity: Medium**

Each agent has a detailed system prompt that frames Claude as a specialist: *"You are an elite code cleanliness architect..."*, *"You are Sentinel, a Senior Security Research Agent..."*. This framing shapes reasoning depth, output quality, and the lens through which code is evaluated. A hook-triggered self-review against a flat rules file loses this persona framing.

**What's lost**: The difference between "check this code against rule CC-002" and "review this code as an elite cleanliness architect who believes clean code is the foundation of maintainable software." The persona prompts create more thorough, opinionated, contextualized feedback.

**Mitigation**: The hook's stderr prompt can include brief framing: *"Review as a security specialist focusing on..."* The rules files can include rationale and philosophy, not just bare rules. But this won't match the depth of a 150+ line specialized system prompt.

**Residual risk**: Medium. Review quality will likely be less nuanced. The most impactful rules (binary violations like forbidden dependencies) won't be affected. Judgment-heavy rules (class cohesion, design smells) will produce less detailed feedback.

### Risk 4: Loss of Holistic Review Across All Changes

**Severity: Medium-High**

An agent reviews all changes as a batch -- it sees the full diff and can identify patterns that emerge across multiple files. A hook fires **per edit**, reviewing each file change in isolation.

**What's lost**: If a developer adds a new Manager in file A and injects it into another Manager in file B, an agent reviewing the full diff catches the Manager->Manager dependency. Two separate hook-triggered reviews might each look fine in isolation -- file A is a valid Manager, file B's edit adds a dependency that looks like any other import.

**Mitigation**: The `/review-idesign` slash command (batch review via `git diff`) catches these cross-file patterns on demand. Developers should run batch review commands before committing. The CLAUDE.md rules also make Claude aware proactively during authoring.

**Residual risk**: Medium. The automatic per-edit hook misses cross-file patterns. The batch slash command catches them but requires manual invocation -- losing the "always-on" guarantee the agents provide.

### Risk 5: Loss of Model Control

**Severity: Low-Medium**

Agents specify their model (`model: opus` or `model: sonnet`). Some agents use opus because their review requires deep reasoning. A hook-triggered self-review runs on whatever model the current session uses.

**What's lost**: If the session runs on sonnet or haiku, security reviews and architectural compliance checks may be less thorough than they would be on opus.

**Mitigation**: This is actually a feature for most rules. The rules extracted for hooks are concrete, binary patterns (forbidden dependencies, method length thresholds, banned code patterns) that don't require opus-level reasoning. The rules that DO need deep reasoning stay with the HYBRID agents for planning sessions. For on-demand deep reviews, the slash commands can be run in an opus session.

**Residual risk**: Low. Binary rules don't need opus. The judgment-heavy work stays with agents.

### Risk 6: Loss of Structured Output Format

**Severity: Low**

Agents produce structured reports: sentinel outputs a finding table with ID, severity, confidence, CWE references, and remediation. Code-cleanliness outputs Issues/Warnings/Praise sections. Hook-triggered self-review produces freeform responses.

**What's lost**: Consistent, structured output that's easy to scan and track over time.

**Mitigation**: The hook's stderr prompt can request a specific format: *"Report violations as: [RULE-ID] [SEVERITY] [FILE:LINE] - description."* The slash commands for batch review can request the full structured format. This gets close to the original but depends on Claude following the format instruction consistently.

**Residual risk**: Low. Format can be specified in the prompt. It may be less consistent but the information content is the same.

### Risk 7: Loss of "Team Review" Dynamic

**Severity: Low-Medium**

The current architecture creates a review team where multiple specialists examine the same change from different angles simultaneously. This multi-perspective review is the core philosophy of agentFactory. With hooks, it becomes Claude reviewing against multiple flat rule lists sequentially -- the specialized lenses and creative tension between agents (e.g., code-cleanliness wanting smaller methods vs. backend-architect wanting comprehensive error handling) is gone.

**What's lost**: The emergent value of multiple independent perspectives. When sentinel and backend-architect review the same code, their findings can complement or tension each other in useful ways. With hooks, it's one Claude reviewing against concatenated rules.

**Mitigation**: This is partially inherent to the hook model and cannot be fully mitigated. However, for high-stakes reviews (pre-commit, pre-PR), the slash commands can invoke multiple rule sets explicitly, and the HYBRID agents can still be invoked manually for planning sessions where multi-perspective input is most valuable.

**Residual risk**: Low-Medium. The "team" value is real but hard to quantify. Most of its concrete value comes from the rules themselves (which are preserved), not from having separate agent personas.

### Risk 8: Advisory Nuance Becomes Binary Enforcement

**Severity: Low**

Code-cleanliness is explicitly described as "advisory -- not a hard block, but always has an opinion." The agent can weigh context: a 22-line method in a complex domain might get a soft warning, while a 22-line method doing simple CRUD gets a stronger push to refactor. Rules in a file tend to be binary: violates or doesn't.

**What's lost**: Contextual judgment about when a rule violation is acceptable.

**Mitigation**: Rules files can include severity levels (Info/Warning/Error) and explicit exceptions: *"CC-001: Methods over 20 lines are a red flag. Exception: complex query builders or state machines may warrant longer methods if they can't be meaningfully decomposed."* The hook prompt can say *"These are advisory guidelines, not hard blocks. Note violations but use judgment about severity in context."*

**Residual risk**: Low. Well-written rules with exceptions and severity levels preserve most of the advisory nuance.

### Summary: What's Actually Lost vs. Preserved

| Capability | Preserved? | Notes |
|------------|-----------|-------|
| Binary rule checking (forbidden patterns, thresholds) | **Fully preserved** | Rules file + hook + CLAUDE.md |
| Per-file review on every edit | **Fully preserved** | Hook triggers automatically |
| Cross-file data flow tracing | **Partially lost** | Hook can prompt for it; slash command catches it; not guaranteed |
| Persistent agent memory | **Partially lost** | Key knowledge moves to CLAUDE.md; auto-accumulation lost |
| Specialized persona/expertise framing | **Partially lost** | Brief framing in hook prompt; less depth than full agent |
| Holistic multi-file review | **Available on demand** | Slash commands provide batch review; not automatic |
| Planning/design sessions | **Fully preserved** | HYBRID agents retained for planning |
| Deep audit workflows (sentinel 8-step) | **Fully preserved** | KEEP/HYBRID agents retained |
| Structured output format | **Mostly preserved** | Hook prompt can request format; less consistent |
| Multi-perspective "team" review | **Partially lost** | Rules preserved; creative tension between personas lost |
| Advisory nuance (soft vs. hard warnings) | **Mostly preserved** | Severity levels and exceptions in rules files |

### Mitigating the Losses: Three-Layer Review Architecture

The original risk assessment assumed PostToolUse hooks are the only mechanism. But Claude Code supports multiple hook types, and the losses can be substantially mitigated by using a **three-layer architecture** instead of a flat hook replacement.

#### Layer 1: CLAUDE.md Rules (Proactive -- During Generation)

**What it solves**: Prevents violations before they happen. This is actually *better* than agent post-review because Claude follows the rules while writing code, not after.

The highest-value rules from each agent go directly into CLAUDE.md. Because CLAUDE.md is loaded into every conversation, Claude applies these rules during code generation without any hook, any agent invocation, or any extra tokens. The violation never gets written in the first place.

**Mitigates**: Specialized persona framing (Risk 3), advisory nuance (Risk 8). The rules in CLAUDE.md carry rationale and context. Claude internalizes them as part of its working instructions, which is closer to "being" the specialist than reviewing against a flat checklist after the fact.

**Token cost**: Zero incremental tokens. CLAUDE.md is already loaded.

#### Layer 2: PostToolUse Hooks (Reactive -- Per Edit)

**What it solves**: Catches violations that slipped through Layer 1. Quick, cheap, per-file checks.

This is the basic hook+rules pattern described in the migration. A bash script checks the file path, and if relevant, tells Claude to self-review against the rules file. Handles binary rules (forbidden patterns, thresholds, banned code) effectively.

**Mitigates**: Per-file rule checking is fully preserved. Structured output (Risk 6) can be addressed with format instructions in the hook prompt.

**Token cost**: Low. Only fires on relevant files. Claude reads a ~2-3 KB rules file and reviews the single edit.

#### Layer 3: PreCommit Hook (Batch -- At Commit Time)

**This is the key missing piece.** Claude Code supports PreCommit hooks that fire when the user runs `git commit`. This is the right time to do expensive, cross-file, holistic review -- the full diff is available, all changes are finalized, and the cost is paid once per commit rather than on every edit.

A PreCommit hook can:

1. Run `git diff --cached --name-only` to get all staged files
2. Group files by domain (backend, frontend, data access)
3. For each relevant domain, exit with code 2 and a stderr prompt that instructs Claude to:
   - Read all changed files as a batch
   - Trace cross-file relationships (controller -> manager -> accessor)
   - Review against the full rules file with specialist framing
   - Report findings in the structured format

**Mitigates**:
- **Cross-file analysis (Risk 1, severity High)**: The PreCommit hook reviews the full diff. It can instruct Claude to trace data flow from controller to accessor, exactly like sentinel's workflow. The prompt can say: *"For each controller change, trace the call chain through managers and accessors. Verify user scoping reaches the data access layer."*
- **Holistic multi-file review (Risk 4, severity Medium-High)**: All changes visible at once. A new Manager in file A and a Manager injection in file B are both in the diff -- the forbidden dependency is caught.
- **Team review dynamic (Risk 7)**: The PreCommit hook can trigger multiple review passes with different specialist framings: *"Review as a security specialist..."*, then *"Review as an architecture compliance auditor..."*. This approximates the multi-perspective team review at commit time.
- **Structured output format (Risk 6)**: The commit-time review has room for a detailed, structured report since it runs once (not per-edit).

**Token cost**: Medium. Runs once per commit, not per edit. A development session with 20 edits but 2 commits pays for the full review twice instead of a watered-down review 20 times. Net savings are still large.

#### How the Three Layers Work Together

```
Developer writes code
        |
        v
 [Layer 1: CLAUDE.md]
 Claude follows rules DURING generation.
 Most violations are prevented before they're written.
 Cost: 0 extra tokens.
        |
        v
 [Layer 2: PostToolUse Hook]
 After each Edit/Write, hook checks file path.
 If relevant, Claude self-reviews the single edit against rules.
 Catches: binary violations, threshold breaches, banned patterns.
 Misses: cross-file patterns, holistic issues.
 Cost: Low (~2-5K tokens per triggered review).
        |
        v
 Developer runs `git commit`
        |
        v
 [Layer 3: PreCommit Hook]
 Reviews full staged diff with specialist framing.
 Traces cross-file relationships.
 Multiple review passes (security, architecture, cleanliness).
 Catches: everything Layers 1-2 missed.
 Cost: Medium (~30-50K tokens, once per commit).
```

Compare this to the current agent model:

```
Current: 5 agents x 10-20K tokens x every review cycle = ~100-200K per feature
Three-layer: ~0 (CLAUDE.md) + ~20K (hooks across edits) + ~40K (PreCommit) = ~60K per feature
```

The three-layer model costs less AND catches cross-file issues that per-edit hooks alone would miss.

#### PreCommit Hook Example

```bash
#!/bin/bash
# .claude/hooks/precommit-review.sh
# PreCommit hook: full multi-perspective review at commit time

# Get staged files
STAGED=$(git diff --cached --name-only)

if [ -z "$STAGED" ]; then
  exit 0
fi

# Check for backend changes
BACKEND_FILES=$(echo "$STAGED" | grep -E '\.(cs|sql)$' | grep -v -E '(Tests?\.cs|/[Tt]ests?/)' || true)
FRONTEND_FILES=$(echo "$STAGED" | grep -E '\.(tsx?|css|scss)$' || true)

REVIEW_PROMPT=""

if [ -n "$BACKEND_FILES" ]; then
  REVIEW_PROMPT="${REVIEW_PROMPT}

## Backend Review
Review ALL staged backend files as a batch. Read each file and trace cross-file relationships.

Files: ${BACKEND_FILES}

1. Check against docs/review-rules/backend-rules.md
2. Check against docs/review-rules/idesign-rules.md -- trace dependencies across Manager/Engine/Accessor files
3. Check against docs/review-rules/sql-rules.md for any data access changes
4. Check against docs/review-rules/security-rules.md -- trace data flow from controllers through to data access, verify user scoping at every layer

Report findings with rule IDs and severity."
fi

if [ -n "$FRONTEND_FILES" ]; then
  REVIEW_PROMPT="${REVIEW_PROMPT}

## Frontend Review
Review ALL staged frontend files as a batch.

Files: ${FRONTEND_FILES}

1. Check against docs/review-rules/react-rules.md
2. Check against docs/review-rules/ui-design-rules.md

Report findings with rule IDs and severity."
fi

if [ -n "$REVIEW_PROMPT" ]; then
  echo "Pre-commit review of all staged changes:${REVIEW_PROMPT}" >&2
  exit 2
fi

exit 0
```

#### Revised Risk Summary With Three-Layer Mitigation

| Capability | PostToolUse Only | With Three-Layer Architecture |
|------------|-----------------|-------------------------------|
| Binary rule checking | **Fully preserved** | **Fully preserved** |
| Per-file review on every edit | **Fully preserved** | **Fully preserved** |
| Cross-file data flow tracing | **Partially lost** | **Mostly preserved** -- PreCommit hook traces full diff |
| Persistent agent memory | **Partially lost** | **Partially lost** -- same; mitigate via CLAUDE.md |
| Specialized persona framing | **Partially lost** | **Mostly preserved** -- CLAUDE.md internalizes rules; PreCommit uses specialist framing |
| Holistic multi-file review | **On demand only** | **Automatic at commit** -- PreCommit reviews full diff |
| Planning/design sessions | **Fully preserved** | **Fully preserved** |
| Deep audit workflows | **Fully preserved** | **Fully preserved** |
| Structured output format | **Mostly preserved** | **Fully preserved** -- PreCommit has room for structured reports |
| Multi-perspective team review | **Partially lost** | **Mostly preserved** -- PreCommit runs multiple review passes |
| Advisory nuance | **Mostly preserved** | **Mostly preserved** |

The three-layer architecture resolves the two highest-severity risks (cross-file analysis and holistic review) by shifting them to commit time rather than eliminating them. The only capability that remains genuinely degraded is **persistent agent memory**, and that's the least impactful loss since most institutional knowledge is derivable from the codebase itself.

### Recommendation

Use the three-layer architecture. It preserves nearly all agent functionality at roughly 30% of the token cost:

- **Layer 1 (CLAUDE.md)**: Actually *improves* on agents by preventing violations during generation instead of catching them after
- **Layer 2 (PostToolUse)**: Catches per-file violations cheaply and automatically
- **Layer 3 (PreCommit)**: Recovers the cross-file analysis and holistic review that per-edit hooks lose
- **HYBRID agents**: Still available for planning sessions and on-demand deep audits when needed

**The key architectural principle**: Automatic hooks catch the cheap, binary violations on every edit (high frequency, low cost). PreCommit hooks catch cross-file and holistic issues at commit time (medium frequency, medium cost). On-demand agent invocations handle the expensive, judgment-heavy reviews when they matter most (lower frequency, justified cost). This is more efficient than running all types of analysis at full cost on every change.

---

## Decision Framework

**Convert to hook+rules when ALL true:**
1. It reacts to file edits (reviewing, validating, or enforcing something after a change)
2. Its logic is rule-based (a checklist of do/don't patterns with concrete examples)
3. It should run automatically on every relevant edit, not just on-demand
4. Claude can do the analysis -- no external API calls or specialized tools needed

**Keep as agent when ANY true:**
1. It performs multi-step workflows (fetching data, calling APIs, orchestrating tools)
2. It runs independently, not in response to an edit
3. It requires tools beyond Read/Grep/Glob
4. It generates new content rather than validating existing content

Most agents have **dual roles** (planning + review). When the review half is rule-based but the planning half requires interactive reasoning, the verdict is **HYBRID**: keep the agent for planning, add a hook for automatic review.

---

## Verdicts

### Original Assessment (Pre-Memory Analysis)

| # | Agent | Verdict | Rationale |
|---|-------|---------|-----------|
| 1 | backend-architect | **HYBRID** | Forbidden/required code patterns are concrete rules for a hook; planning (interface design, pattern selection) stays as agent |
| 2 | react-architect | **HYBRID** | 7 anti-patterns with numeric thresholds are rule-checkable; component tree design is generative |
| 3 | sentinel | **HYBRID** | 6 Scary Patterns are high-value automatic rules; full 8-step audit requires multi-file tracing as agent |
| 4 | code-cleanliness | **CONVERT** | Purely rule-based review with numeric thresholds and bad/good examples; no meaningful planning role |
| 5 | idesign-architect | **HYBRID** | 3 forbidden dependencies are binary and Grep-checkable; layer placement decisions stay as agent |
| 6 | azure-architect | **KEEP** | Multi-step workflows, Azure CLI, Bicep generation, self-maintenance protocol |
| 7 | sql-data-architect | **HYBRID** | 8 anti-patterns with code examples are rule-checkable; schema design is generative |
| 8 | table-storage-architect | **HYBRID** | Hard rules (no table scans, handle continuation tokens) are binary; partition key design stays as agent |
| 9 | data-clarifier | **KEEP** | Entirely generative, on-demand, no concrete do/don't rules |
| 10 | debug-investigator | **KEEP** | On-demand 7-phase workflow, requires Bash/WebSearch, reacts to bugs not edits |
| 11 | modern-ui-agent | **HYBRID** | 8 anti-patterns and 10-item review checklist are rule-checkable; design sessions stay as agent |

**Original totals: 1 CONVERT, 3 KEEP, 7 HYBRID**

### Revised Assessment (Post-Memory Analysis)

The memory analysis changes the calculus in two ways:

**1. "Memory retention" is no longer a reason to keep an agent as HYBRID.**

The original assessment partially justified HYBRID verdicts with "the agent keeps its memory for planning." But the two-tier memory model shows that ~70% of what agents store in memory is project-level knowledge that should be in CLAUDE.md anyway. The remaining ~30% (agent-specific observations like violation hotspots) is useful but not load-bearing -- it's nice-to-have, not a reason to keep a dedicated agent running.

This means the HYBRID verdict for each agent now rests entirely on whether its **planning role** is genuinely generative and can't be captured in CLAUDE.md rules. Some hold up. Others don't.

**2. Agents WITHOUT memory actually get BETTER under the hook+rules model.**

Six agents (backend-architect, sentinel, idesign-architect, azure-architect, debug-investigator, modern-ui-agent) have no memory. Under the current model, they start fresh every session with zero accumulated knowledge. Under the hook+rules model, their key rules live in CLAUDE.md (always loaded, session after session) and rules files (read on demand). This is strictly better than the current state for these agents -- they gain persistence they never had.

### Agents That Change Verdict

#### idesign-architect: HYBRID → CONVERT

**Previous reasoning**: "3 forbidden dependencies are binary and Grep-checkable; layer placement decisions stay as agent."

**Why it changes**: The idesign-architect has no memory (`memory:` is not set). Its "planning role" is deciding which layer new classes belong in -- but these rules are entirely concrete and documentable:

- Controllers call Managers only
- Managers may inject Engines and Accessors
- Engines may inject Accessors
- Accessors may inject SDK clients and infrastructure

This is a lookup table, not interactive reasoning. It doesn't need a dedicated agent to answer "where does this new class go?" -- CLAUDE.md rules with the layer definitions and dependency direction table are sufficient. The agent's Review Workflow (lines 56-64) is literally a Grep-based search process. Its Violation Detection Methodology (lines 68-90) describes exactly the kind of mechanical check that hooks handle.

Compare this to backend-architect's planning role (designing interfaces, selecting GoF patterns, proposing SOLID-compliant architectures) which genuinely requires interactive design reasoning.

**New verdict**: **CONVERT**. Full IDesign rules in `docs/review-rules/idesign-rules.md`, layer definitions and dependency direction in CLAUDE.md, PostToolUse hook on `*Manager.cs`/`*Engine.cs`/`*Accessor.cs`, PreCommit hook for cross-file dependency tracing. No agent retained.

#### modern-ui-agent: HYBRID → KEEP (verdict unchanged but reasoning shifts)

**Previous reasoning**: "Anti-patterns + checklist → rules; design sessions stay as agent."

**Why memory matters**: This agent has no memory, so the two-tier model doesn't directly apply. However, the analysis reinforces that its design system knowledge (color architecture, typography scale, spacing system, component patterns) is extensive reference material that would benefit from being in CLAUDE.md or a referenced design system doc -- always available during code generation, not just when the agent is manually invoked.

**Revised reasoning**: The anti-patterns and review checklist move to rules+hooks. The design system foundation (337 lines of detailed patterns, color values, spacing systems, component specifications) is too large for CLAUDE.md but should be a referenced doc. The agent's primary value is in active design sessions -- creating new designs, not reviewing existing code. This is generative work that can't be rule-based.

**Verdict stays KEEP** but with a change: extract anti-patterns and review checklist to rules+hooks, extract design system reference to a standalone doc that CLAUDE.md points to, keep the agent slimmed down for active design sessions only.

### Agents That Don't Change Verdict (But Reasoning Strengthens)

#### code-cleanliness: CONVERT (strengthened)

Memory was the last argument against full conversion. The two-tier analysis shows its memory content is ~80% project-wide knowledge (style conventions, patterns) that belongs in CLAUDE.md, and ~20% violation hotspots that can be captured as project-specific notes in the rules file. No reason to keep the agent.

#### backend-architect: HYBRID (unchanged)

No memory. Planning role (interface design, GoF pattern selection, SOLID architecture proposals) is genuinely generative and can't be captured as rules. The rules+hooks handle the review role. Memory analysis doesn't affect this -- the agent never had memory to lose.

#### react-architect: HYBRID (unchanged, but memory scope narrows)

Has memory, but the two-tier analysis shows most of what it stores (component conventions, hook patterns, state management approach) belongs in CLAUDE.md. Agent memory retains only observations like "the dashboard feature has excessive re-renders." Planning role (component tree design, state maps) is genuinely generative.

#### sentinel: HYBRID (unchanged)

No memory. The 8-step audit methodology requires multi-file tracing and deep reasoning that can't be rule-based. The 6 Scary Patterns move to hooks. Memory analysis doesn't affect this.

#### sql-data-architect: HYBRID (unchanged, but memory scope narrows)

Has memory. Schema decisions and indexing strategies move to CLAUDE.md. Agent memory retains only performance findings and regression warnings. Planning role (schema design, migration planning, CQRS architecture) is genuinely generative.

#### table-storage-architect: HYBRID (unchanged, but memory scope narrows)

Has memory. Partition key strategies move to CLAUDE.md. Agent memory retains only query pattern observations. Planning role (partition key design from access patterns, denormalization strategy) is genuinely generative.

#### azure-architect, data-clarifier, debug-investigator: KEEP (unchanged)

These verdicts were based on workflow complexity, tool requirements, and generative roles -- none of which are affected by memory analysis.

### Revised Verdict Table

| # | Agent | Original | Revised | Change | Key Reason for Change |
|---|-------|----------|---------|--------|----------------------|
| 1 | backend-architect | HYBRID | **HYBRID** | — | Planning role is genuinely generative |
| 2 | react-architect | HYBRID | **HYBRID** | — | Planning role is generative; memory scope narrows |
| 3 | sentinel | HYBRID | **HYBRID** | — | 8-step audit requires deep multi-file reasoning |
| 4 | code-cleanliness | CONVERT | **CONVERT** | Strengthened | Memory was last argument against; two-tier resolves it |
| 5 | idesign-architect | HYBRID | **CONVERT** | Changed | Planning role is a concrete lookup table, not interactive reasoning; no memory |
| 6 | azure-architect | KEEP | **KEEP** | — | Multi-step workflows, CLI, content generation |
| 7 | sql-data-architect | HYBRID | **HYBRID** | — | Schema design is generative; memory scope narrows |
| 8 | table-storage-architect | HYBRID | **HYBRID** | — | Partition key design is generative; memory scope narrows |
| 9 | data-clarifier | KEEP | **KEEP** | — | Entirely generative, on-demand |
| 10 | debug-investigator | KEEP | **KEEP** | — | On-demand workflow, requires Bash/WebSearch |
| 11 | modern-ui-agent | HYBRID | **KEEP** | Reclassified | Design sessions are generative; extract anti-patterns to hooks, design system to reference doc |

**Revised totals: 2 CONVERT, 4 KEEP, 5 HYBRID**

### Impact on Token Savings

Adding idesign-architect to CONVERT eliminates another opus-model agent invocation from every backend review cycle. The revised architecture:

- **Original estimate**: ~200K tokens/feature → ~60K tokens/feature (70% savings)
- **Revised estimate**: ~200K tokens/feature → ~50K tokens/feature (75% savings)

The extra savings come from:
- One fewer agent invocation per planning session (idesign rules now in CLAUDE.md)
- One fewer agent invocation per review cycle (idesign checks now in hooks)
- Smaller memory files across all HYBRID agents (Tier 2 only, well under 200 lines)

---

## What to Change: CONVERT Agents

### code-cleanliness (FULL CONVERSION)

**Delete**: `agents/code-cleanliness.md`

**Create**: `docs/review-rules/code-cleanliness-rules.md` with these rules:

| ID | Severity | Rule | Bad Example | Good Example |
|----|----------|------|-------------|--------------|
| CC-001 | Warning/Error | Methods over 15 lines flagged; over 20 lines is a red flag. Count executable lines only (skip blanks, braces, comments). | A 25-line method doing validation + mapping + persistence | Three 8-line methods: `Validate()`, `MapToEntity()`, `Persist()` |
| CC-002 | Error | Nested if statements forbidden. Use guard clauses, pattern matching, or extraction. | `if (a) { if (b) { if (c) { ... } } }` | Early returns: `if (!a) return; if (!b) return; if (!c) return; ...` |
| CC-003 | Warning | Constructor parameters must be 4 or fewer. More suggests the class has too many responsibilities. | `ctor(IFoo, IBar, IBaz, IQux, IQuux, ICorge)` | Split into two classes with 3 dependencies each |
| CC-004 | Info | Single-implementation interfaces are a smell. Question whether the interface adds value. | `IFooService` with only `FooService` | Use the concrete class directly, or justify the interface (testability, DI boundary) |
| CC-005 | Warning | Prefer declarative style: LINQ over imperative loops, immutable records over mutable state, switch expressions over verbose conditionals. | `foreach` loop building a list with `if` checks | `.Where().Select().ToList()` |
| CC-006 | Warning | Class cohesion: all methods should use most fields. If methods cluster into groups using different field subsets, split the class. | Class with 8 fields where half the methods use fields 1-4 and the other half use fields 5-8 | Two classes, each with 4 fields and high cohesion |
| CC-007 | Info | No deep inheritance hierarchies or abstract base classes without clear justification. Prefer composition, delegates, or simple functions. | `AbstractBaseService<T> : BaseEntityService<T> : IService<T>` | A concrete class using composition or a `Func<>` parameter |

**Create**: PostToolUse hook (see [Hook Architecture](#hook-architecture) below)

**Add to CLAUDE.md**:
```
## Code Cleanliness Rules (Auto-Enforced)
- Methods MUST be under 20 lines of executable code; aim for under 15
- Nested if statements are forbidden -- use guard clauses or early returns
- Constructor parameters must be 4 or fewer
- Prefer declarative style: LINQ, switch expressions, pattern matching, immutable records
- See docs/review-rules/code-cleanliness-rules.md for the full rule set
```

**Create**: `/review-cleanliness` slash command that runs `git diff` and reviews all changed files against the rules file.

---

## What to Change: HYBRID Agents

For each HYBRID agent, two things happen:
1. **Extract review rules** into a rules file and wire up a hook
2. **Slim down the agent** to remove the review checklist sections, keeping only planning/design content

### backend-architect

**Create**: `docs/review-rules/backend-rules.md`

| ID | Severity | Rule |
|----|----------|------|
| BE-001 | Error | No silent fallbacks: `??` with default values is forbidden on data lookups. Throw `NotFoundException` instead. |
| BE-002 | Error | No swallowed exceptions: `catch { return default; }` is forbidden. Log and rethrow. |
| BE-003 | Error | No `TrySomethingSafe()` methods that never fail. Failures must be explicit. |
| BE-004 | Warning | All public APIs must have XML doc comments. |
| BE-005 | Warning | Use async/await for all I/O operations. |
| BE-006 | Warning | Implement proper disposal patterns (`IDisposable`/`IAsyncDisposable`) for resources. |
| BE-007 | Info | Methods should be under 30 lines. |
| BE-008 | Warning | Dependencies must be explicit, minimal, and constructor-injected. |

Extract bad/good code examples from lines 41-68 and 117-124 of `backend-architect.md`.

**Hook filter**: `Edit`/`Write` on `**/*.cs` (exclude `**/*Tests.cs`, `**/*.Test.cs`, `**/Tests/**`)

**Slim the agent**: Remove the "Red Flag Patterns" section (lines 117-124) and "Self-Verification" checklist (lines 102-115). Keep the planning methodology, SOLID principles, GoF patterns, and "When Making Changes" sections.

**Add to CLAUDE.md**:
```
## Backend Rules (Auto-Enforced)
- NEVER use silent fallbacks (`??` with defaults, catch-return-default, TrySomething patterns)
- Failures must be explicit: throw exceptions, log and rethrow
- See docs/review-rules/backend-rules.md for the full rule set
```

### react-architect

**Create**: `docs/review-rules/react-rules.md`

| ID | Severity | Rule |
|----|----------|------|
| RC-001 | Error | No prop drilling beyond 2 levels. Use composition or context. |
| RC-002 | Warning | Components over 200 lines likely need splitting. |
| RC-003 | Error | Never use `useEffect` for derived state. Use `useMemo` instead. |
| RC-004 | Error | No state synchronization between two sources. Redesign the architecture. |
| RC-005 | Warning | Business logic must not live in components. Extract to hooks or utility functions. |
| RC-006 | Warning | Data fetching in `useEffect` must have proper cleanup/cancellation. |
| RC-007 | Warning | No circular dependencies between feature modules. |
| RC-008 | Error | No `any` types in TypeScript. Use proper types or `unknown`. |

Extract from lines 99-107 of `react-architect.md`.

**Hook filter**: `Edit`/`Write` on `**/*.tsx`, `**/*.ts` in frontend source directories

**Slim the agent**: Remove the "Anti-Patterns to Flag" section (lines 99-107). Keep component hierarchy design, state management strategy, methodology, and output format sections.

### sentinel

**Create**: `docs/review-rules/security-rules.md`

| ID | Severity | Rule |
|----|----------|------|
| SEC-001 | Critical | Direct entity lookups by ID on user-owned entities MUST include user scoping predicate. |
| SEC-002 | Critical | Never bypass user scoping filters on user-owned entities. |
| SEC-003 | Critical | UserId must come from authenticated identity, NEVER from request body/query/route input. |
| SEC-004 | Critical | All ID-based read/update/delete operations must verify user/owner ownership. |
| SEC-005 | High | All list/search/export/report/download endpoints must enforce user scoping. |
| SEC-006 | High | All endpoints touching user data must be authenticated. No anonymous access to user data. |

Extract from "Scary Pattern" Rules (lines 103-110) of `sentinel.md`.

**Hook filter**: `Edit`/`Write` on `**/*.cs`, `**/*.tsx`, `**/*.ts` (full-stack, security is always relevant)

**Keep the agent intact** for its full 8-step audit methodology. The hook handles quick pattern catches; the agent handles deep audits. Do NOT remove sections from the agent.

### idesign-architect

**Create**: `docs/review-rules/idesign-rules.md`

| ID | Severity | Rule |
|----|----------|------|
| ID-001 | Critical | FORBIDDEN: Manager class injecting or calling another Manager. |
| ID-002 | Critical | FORBIDDEN: Engine class injecting or calling another Engine. |
| ID-003 | Critical | FORBIDDEN: Accessor class injecting or calling another Accessor. |
| ID-004 | Error | Dependencies must flow downward only: Controller -> Manager -> Engine -> Accessor. |
| ID-005 | Warning | Design smell: Forks -- a Manager branching into two unrelated workflows should be split. |
| ID-006 | Warning | Design smell: Staircases -- call chains like Manager->Engine->Accessor->Engine->Accessor should be flattened. |
| ID-007 | Warning | Design smell: Functional decomposition -- breaking a Manager into sub-Managers violates ID-001. |

Include infrastructure whitelist (ILogger, IConfiguration, IOptions, SDK clients are NOT violations).

**Hook filter**: `Edit`/`Write` on `**/*Manager.cs`, `**/*Engine.cs`, `**/*Accessor.cs`

**Slim the agent**: Remove the "Violation Detection Methodology" section (lines 68-90) which is now the hook's job. Keep the layer definitions, planning guidance, feedback format, and communication style.

### sql-data-architect

**Create**: `docs/review-rules/sql-rules.md`

| ID | Severity | Rule |
|----|----------|------|
| SQL-001 | Error | NEVER use GUIDs as clustered primary keys. Use `int IDENTITY` or `bigint IDENTITY`. |
| SQL-002 | Error | NEVER call `Database.Migrate()` on application startup. |
| SQL-003 | Warning | All schema changes must follow expand-contract pattern for zero-downtime. |
| SQL-004 | Error | Never rename or drop columns directly in production. |
| SQL-005 | Warning | Use `AsNoTracking()` for all read-only queries. |
| SQL-006 | Warning | Use keyset pagination. NEVER use `OFFSET/FETCH` at high page numbers. |
| SQL-007 | Info | Use `.Select()` projections for reads, not full entity loads. |

**Hook filter**: `Edit`/`Write` on `**/*Accessor.cs`, `**/Migrations/**`, `**/*DbContext.cs`, `**/*.sql`

**Slim the agent**: Remove anti-patterns table. Keep schema design philosophy, migration strategy, indexing guidance, and review methodology.

### table-storage-architect

**Create**: `docs/review-rules/table-storage-rules.md`

| ID | Severity | Rule |
|----|----------|------|
| TS-001 | Critical | NEVER use a single constant PartitionKey for all entities (creates a hot partition). |
| TS-002 | Error | NEVER use GUIDs as PartitionKeys (destroys locality). |
| TS-003 | Error | Table scans are FORBIDDEN in production code. All queries must specify PartitionKey. |
| TS-004 | Error | Always handle continuation tokens. Never assume a query returns all results. |
| TS-005 | Warning | Always use ETag for conditional updates (optimistic concurrency). |
| TS-006 | Warning | Use query projection -- select only the properties you need. |

**Hook filter**: `Edit`/`Write` on Table Storage entity and accessor classes (project-specific path pattern)

**Slim the agent**: Remove anti-patterns table. Keep partition key design philosophy, mental model shift table, and design principles.

### modern-ui-agent

**Create**: `docs/review-rules/ui-design-rules.md`

| ID | Severity | Rule |
|----|----------|------|
| UI-001 | Warning | No saturated primary sidebars (solid blue/purple navs). Use neutral backgrounds. |
| UI-002 | Warning | No rainbow inconsistency. Establish a strict 2-3 color system beyond neutrals. |
| UI-003 | Info | Replace heavy borders with subtle shadows or background differentiation. |
| UI-004 | Warning | Create clear typographic hierarchy -- at least 3-4 distinct levels. No uniform text sizing. |
| UI-005 | Warning | Reserve saturated color for ONE primary action per view. Not every button is primary. |
| UI-006 | Error | Every interactive element MUST have hover, active, and focus states. |
| UI-007 | Warning | No sparse/empty pages. Use intentional whitespace with appropriate content density. |
| UI-008 | Warning | Status communication must use consistent color coding (success/warning/danger/info). |

Extract from anti-patterns (lines 25-35) and review checklist (lines 315-326) of `modern-ui-agent.md`.

**Hook filter**: `Edit`/`Write` on `**/*.tsx`, `**/*.css`, `**/*.scss` in frontend directories

**Slim the agent**: Remove the "Review Checklist" section (lines 315-326) which is now the hook's job. Keep the design system foundation, component patterns, and application process.

---

## What to Change: KEEP Agents

No changes needed for these agents:

- **azure-architect** -- Multi-step workflows, Azure CLI, Bicep generation, self-maintenance
- **data-clarifier** -- Entirely generative, on-demand, contextual judgment
- **debug-investigator** -- On-demand 7-phase workflow, requires Bash/WebSearch

---

## Hook Architecture

**Recommended: Domain-grouped hooks** (4 hook scripts in `.claude/hooks/`)

| Hook Script | Rules Files Loaded | File Filter |
|-------------|-------------------|-------------|
| `review-backend.sh` | backend-rules.md, idesign-rules.md, sql-rules.md, table-storage-rules.md | `**/*.cs`, `**/*.sql` |
| `review-frontend.sh` | react-rules.md, ui-design-rules.md | `**/*.tsx`, `**/*.ts`, `**/*.css` |
| `review-security.sh` | security-rules.md | `**/*.cs`, `**/*.tsx`, `**/*.ts` |
| `review-cleanliness.sh` | code-cleanliness-rules.md | `**/*.cs`, `**/*.tsx`, `**/*.ts` |

Each hook script:
1. Reads the PostToolUse JSON from stdin
2. Checks if `tool` is `Edit` or `Write`
3. Checks if the file path matches its filter pattern
4. If relevant, exits with code 2 and writes to stderr: `"Review the edit you just made against the rules in docs/review-rules/<rules-file>.md. Report any violations with rule IDs."`
5. If not relevant, exits with code 0 (no feedback)

### Hook Script Template

```bash
#!/bin/bash
# .claude/hooks/review-backend.sh
# PostToolUse hook: triggers Claude self-review for backend edits

INPUT=$(cat)
TOOL=$(echo "$INPUT" | jq -r '.tool_name // .tool // empty')
FILE=$(echo "$INPUT" | jq -r '.tool_input.file_path // .tool_input.file // empty')

# Only trigger on Edit/Write
if [[ "$TOOL" != "Edit" && "$TOOL" != "Write" ]]; then
  exit 0
fi

# Only trigger on backend files
case "$FILE" in
  *.cs|*.sql)
    ;;
  *)
    exit 0
    ;;
esac

# Skip test files
case "$FILE" in
  *Tests.cs|*Test.cs|*/Tests/*|*/tests/*)
    exit 0
    ;;
esac

# Trigger Claude self-review
echo "Review the edit you just made to $FILE against the rules in docs/review-rules/backend-rules.md and docs/review-rules/idesign-rules.md. Report any violations with rule IDs and severity." >&2
exit 2
```

### hooks.json Configuration

Add to `.claude/hooks.json`:
```json
{
  "hooks": {
    "PostToolUse": [
      { "command": ".claude/hooks/review-backend.sh" },
      { "command": ".claude/hooks/review-frontend.sh" },
      { "command": ".claude/hooks/review-security.sh" },
      { "command": ".claude/hooks/review-cleanliness.sh" }
    ]
  }
}
```

---

## CLAUDE.md Architecture: Lean Index + Reference Files

### The Problem: CLAUDE.md Bloat

CLAUDE.md is loaded into **every** conversation's context. If we pack all rules, design systems, layer definitions, project knowledge, and agent workflow instructions into it, it becomes a massive token tax on every interaction -- including ones that have nothing to do with those rules.

Consider what the migration wants to put in CLAUDE.md:
- ~15-20 inlined rules across 8 domains (~60 lines)
- IDesign layer definitions and dependency table (~30 lines, now that idesign-architect is CONVERT)
- Modern UI design system reference (~20 lines minimum, 337 lines if full)
- Project knowledge promoted from agent memory (~40-80 lines)
- Agent workflow instructions (~30 lines)
- Existing project context (tech stack, build commands, etc.)

That easily reaches 200-400 lines before any project-specific content. A developer asking "fix this CSS bug" pays the token cost of loading IDesign layer definitions and SQL anti-patterns they'll never need.

### The Solution: CLAUDE.md as Lean Index

CLAUDE.md should contain only two things:

1. **Universal rules** -- the 5-10 rules that apply to literally every edit regardless of domain (e.g., "no silent fallbacks," "no nested ifs"). These are worth the always-loaded cost.
2. **Pointers to reference files** -- short descriptions with file paths that tell Claude where to look when working in a specific domain. Claude reads these on demand using the Read tool.

Everything else lives in reference files that Claude reads **only when relevant**.

### Reference File Structure

```
docs/
├── review-rules/                    # Hook-triggered review rules
│   ├── backend-rules.md             # BE-001 through BE-008
│   ├── react-rules.md               # RC-001 through RC-008
│   ├── security-rules.md            # SEC-001 through SEC-006
│   ├── idesign-rules.md             # ID-001 through ID-007 + layer definitions
│   ├── sql-rules.md                 # SQL-001 through SQL-007
│   ├── table-storage-rules.md       # TS-001 through TS-006
│   ├── code-cleanliness-rules.md    # CC-001 through CC-007
│   └── ui-design-rules.md           # UI-001 through UI-008
├── design-system.md                 # Full design system (from modern-ui-agent)
└── architecture.md                  # IDesign layers, dependency direction, project architecture
```

### CLAUDE.md Template (Lean Version)

```markdown
## Universal Rules (All Code)

- NEVER use silent fallbacks (`??` with defaults, catch-return-default, TrySomething patterns)
- Methods must be under 20 lines of executable code; no nested if statements
- Constructor parameters must be 4 or fewer
- Every query on user-owned entities MUST include a user identity predicate
- UserId must come from authenticated identity, NEVER from request input

## Reference Files

Read these files when working in the relevant domain. Do NOT read files for
domains you are not currently working in.

| Domain | Reference File | When to Read |
|--------|---------------|--------------|
| Backend (.cs) | `docs/review-rules/backend-rules.md` | Writing or reviewing C# code |
| React (.tsx/.ts) | `docs/review-rules/react-rules.md` | Writing or reviewing React components |
| Security | `docs/review-rules/security-rules.md` | Writing controllers, auth, data access |
| Architecture | `docs/review-rules/idesign-rules.md` | Creating or modifying Manager/Engine/Accessor classes |
| SQL / EF Core | `docs/review-rules/sql-rules.md` | Writing migrations, queries, DbContext changes |
| Table Storage | `docs/review-rules/table-storage-rules.md` | Writing Table Storage entities or accessors |
| Code cleanliness | `docs/review-rules/code-cleanliness-rules.md` | Any code authoring (all languages) |
| UI Design | `docs/review-rules/ui-design-rules.md` | Styling, layout, component visual design |
| Design System | `docs/design-system.md` | Creating new UI components or pages |
| Architecture Overview | `docs/architecture.md` | Understanding layer boundaries and project structure |
```

### How This Works with the Three Layers

**Layer 1 (CLAUDE.md)**: Only the ~5 universal rules are always loaded. Claude follows these during generation with zero extra cost.

**Layer 2 (PostToolUse hooks)**: The hook's stderr prompt tells Claude which rules file to read: *"Review this edit against docs/review-rules/backend-rules.md."* Claude reads the file on demand -- tokens are spent only when the hook fires on a relevant file.

**Layer 3 (PreCommit hook)**: The PreCommit prompt tells Claude to read all rules files relevant to the staged changes. Tokens are spent once per commit.

**On-demand (agents)**: HYBRID agents for planning sessions read the reference files they need. The agent prompt can say *"Read docs/architecture.md for project context before designing."*

### Token Impact

| Approach | CLAUDE.md Size | Per-Conversation Cost |
|----------|---------------|----------------------|
| Everything inlined | ~300-400 lines | ~6-8K tokens on every conversation |
| Lean index + references | ~30-40 lines | ~600-800 tokens + on-demand reads only when relevant |

A developer fixing a CSS bug loads ~600 tokens of CLAUDE.md instead of ~8K. A developer writing a new Manager loads ~600 tokens of CLAUDE.md + ~2K tokens of idesign-rules.md (read on demand) = ~2.6K total. Both are significantly cheaper than the inlined approach.

### What the Hooks Need to Change

The PostToolUse hook stderr message must explicitly tell Claude to read the rules file, since it's no longer inlined in CLAUDE.md:

```bash
# Before (rules inlined in CLAUDE.md -- Claude already knows them):
echo "Review this edit for backend rule violations." >&2

# After (rules in reference file -- Claude needs to read it):
echo "Read docs/review-rules/backend-rules.md then review the edit you just made to $FILE. Report any violations with rule IDs." >&2
```

This adds one Read tool call per hook trigger -- a small cost that's far outweighed by the savings from not loading all rules on every conversation.

---

## Agent Memory Analysis

### Current Memory Design

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

### How Memory Is Supposed to Compound

The methodology (`agent-architecture.md`, Principle 6) states:

> *"Compound knowledge. Agents with memory build institutional knowledge across sessions. Patterns discovered today inform reviews tomorrow."*

The intended lifecycle:
1. Agent is invoked → loads `MEMORY.md` into system prompt
2. Agent reviews code → discovers a pattern (e.g., "reporting module tends to have 25+ line methods")
3. Agent writes the discovery to `MEMORY.md`
4. Next session → agent loads updated memory → knows to pay extra attention to the reporting module
5. Over time, reviews become more targeted and efficient

### Problems with the Current Memory Design

#### Problem 1: Knowledge Is Siloed by Agent

Code-cleanliness discovers "the reporting module has long methods." React-architect discovers "the reporting dashboard uses prop drilling." SQL-data-architect discovers "the reporting queries have N+1 issues." These are all about the same module but stored in three separate memory files that never cross-reference.

No agent sees the full picture. No mechanism promotes cross-agent knowledge. The main Claude session doesn't see any of it.

#### Problem 2: ~70% of Stored Knowledge Should Be in CLAUDE.md

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

#### Problem 3: Memory Adds Tokens on Every Invocation, Including Wasted Exits

Each memory-enabled agent loads its full `MEMORY.md` (up to 200 lines) on every invocation. When code-cleanliness is invoked for a review and exits with "no findings" because only infrastructure files changed, it still loaded its memory. As memory accumulates toward the 200-line limit, this becomes a meaningful token cost for zero-value invocations.

With 5 memory-enabled agents on the review team, a single review cycle could load up to 1,000 lines of memory content (~5 x 200 lines) before any code is even read. On "no findings" exits, every one of those tokens is wasted.

#### Problem 4: No Quality Control on Stored Knowledge

Agents self-curate with the instruction "update or remove memories that turn out to be wrong or outdated." But there's no verification mechanism. An agent could store an incorrect observation (e.g., "this project follows repository pattern" when it actually uses IDesign accessors) that persists across sessions and subtly misguides future reviews.

Unlike CLAUDE.md, which is version-controlled and human-reviewed, agent memory files accumulate silently. Nobody reviews what agents write to their memory unless they go looking.

#### Problem 5: Memory Is Orphaned by Migration

When code-cleanliness is deleted (CONVERT verdict), its accumulated memory about recurring violations in specific modules is lost. The rules+hooks replacement has no memory mechanism. Knowledge that took 20 sessions to accumulate disappears.

For HYBRID agents that are slimmed down, the memory survives (the agent still exists for planning), but the review-specific observations ("this module always fails CC-002") are no longer relevant since the agent no longer does reviews.

### Recommendation: Two-Tier Memory Model

Instead of agent-scoped memory that siloes knowledge, restructure into two tiers:

#### Tier 1: Project Knowledge → CLAUDE.md

Visible to all agents, all hooks, and the main session. Contains:
- Code conventions and style patterns
- Component architecture patterns and hook conventions
- Schema design decisions and indexing strategies
- Partition key strategies and data model decisions
- UI layout preferences and design system choices
- Any knowledge that multiple agents or the main session would benefit from

This is the "institutional knowledge" that the methodology promises -- but placed where it actually compounds across the entire system, not just within one agent.

#### Tier 2: Agent-Specific Observations → Agent Memory

Visible to one agent only. Contains only things that are genuinely agent-specific:
- Recurring violation hotspots ("reporting module tends to violate CC-001")
- Performance regression warnings ("OrderAccessor N+1 was fixed in PR #42, watch for regression")
- Module-specific problem patterns ("the auth middleware has complex branching that resists cleanup")

This keeps memory files small (well under 200 lines), reduces wasted tokens on irrelevant invocations, and ensures the most valuable knowledge is in CLAUDE.md where it reaches everything.

#### Impact on Migration

| Agent | Verdict | Memory Action |
|-------|---------|--------------|
| code-cleanliness | CONVERT (delete) | Migrate project knowledge to CLAUDE.md. Violation hotspot knowledge → project-specific notes in rules file. Memory directory deleted. |
| react-architect | HYBRID | Keep memory for planning. Move project-wide patterns (component conventions) to CLAUDE.md. Memory file becomes smaller, agent-specific only. |
| sql-data-architect | HYBRID | Keep memory for planning. Move schema decisions to CLAUDE.md. Memory retains only performance findings and regression warnings. |
| table-storage-architect | HYBRID | Keep memory for planning. Move partition strategies to CLAUDE.md. Memory retains only query pattern observations. |
| data-clarifier | KEEP | No changes. Agent and memory stay as-is. |

#### What Changes in the Bootstrap

`bootstrap-agents.md` Phase 5 should be updated:
- Add guidance distinguishing Tier 1 (CLAUDE.md) vs. Tier 2 (agent memory) knowledge
- Instruct agents to write project-wide discoveries to CLAUDE.md (or flag them for promotion)
- Agent memory instructions should say: *"Store only agent-specific observations here. Project-wide conventions and patterns belong in CLAUDE.md."*

`agent-architecture.md` Step 5 and Principle 6 should be updated:
- Describe the two-tier model
- Clarify that "institutional knowledge" compounds in CLAUDE.md, not just in agent memory
- Update the memory instruction template in agent definitions

---

## Migration Checklist

### Phase 1: Create rules files (no agent changes yet)
- [ ] Create `docs/review-rules/` directory
- [ ] Create `code-cleanliness-rules.md` with CC-001 through CC-007
- [ ] Create `backend-rules.md` with BE-001 through BE-008
- [ ] Create `react-rules.md` with RC-001 through RC-008
- [ ] Create `security-rules.md` with SEC-001 through SEC-006
- [ ] Create `idesign-rules.md` with ID-001 through ID-007
- [ ] Create `sql-rules.md` with SQL-001 through SQL-007
- [ ] Create `table-storage-rules.md` with TS-001 through TS-006
- [ ] Create `ui-design-rules.md` with UI-001 through UI-008

### Phase 2: Create hooks, reference docs, and CLAUDE.md template
- [ ] Create `.claude/hooks/review-backend.sh` (stderr prompts Claude to read rules file)
- [ ] Create `.claude/hooks/review-frontend.sh`
- [ ] Create `.claude/hooks/review-security.sh`
- [ ] Create `.claude/hooks/review-cleanliness.sh`
- [ ] Create `.claude/hooks/precommit-review.sh`
- [ ] Create `hooks.json` configuration (PostToolUse + PreCommit)
- [ ] Create `docs/design-system.md` (extracted from modern-ui-agent)
- [ ] Create `docs/architecture.md` (IDesign layers + project structure)
- [ ] Create lean CLAUDE.md template (~30-40 lines): 5 universal rules + reference file table

### Phase 3: Restructure agent memory
- [ ] Update memory instructions in all 5 memory-enabled agents to distinguish Tier 1 (→ CLAUDE.md) vs. Tier 2 (→ agent memory) knowledge
- [ ] Rewrite "Persistent Agent Memory" sections to say: *"Store only agent-specific observations here (recurring hotspots, regression warnings). Project-wide conventions and patterns belong in CLAUDE.md."*
- [ ] Add domain-specific guidance for what stays in memory vs. what gets promoted to CLAUDE.md
- [ ] Update `agent-architecture.md` Step 5 and Principle 6 to describe two-tier memory model
- [ ] Update `bootstrap-agents.md` Phase 5 to include memory tier guidance

### Phase 4: Update agents
- [ ] Delete `agents/code-cleanliness.md` (fully replaced by rules+hooks)
- [ ] Slim `agents/backend-architect.md` -- remove review checklists, keep planning content
- [ ] Slim `agents/react-architect.md` -- remove anti-patterns list, keep design content
- [ ] Slim `agents/idesign-architect.md` -- remove violation detection methodology, keep planning
- [ ] Slim `agents/sql-data-architect.md` -- remove anti-patterns table, keep design content
- [ ] Slim `agents/table-storage-architect.md` -- remove anti-patterns table, keep design content
- [ ] Slim `agents/modern-ui-agent.md` -- remove review checklist, keep design system content
- [ ] Keep `agents/sentinel.md` intact (hook supplements, doesn't replace)

### Phase 5: Update methodology docs
- [ ] Update `agent-architecture.md` to describe three-layer review architecture and two-tier memory model
- [ ] Update `bootstrap-agents.md` to include hook setup steps and memory tier guidance
- [ ] Document when to use the agent vs. relying on the hook

### Phase 6: Verification
- [ ] Test each hook by editing a file and confirming self-review triggers
- [ ] Verify CLAUDE.md rules are followed proactively during code generation
- [ ] Run a side-by-side comparison: old agent review vs. hook+rules review on the same change
- [ ] Confirm HYBRID agents still work for their planning role with review sections removed
- [ ] Verify memory-enabled agents store only Tier 2 knowledge in agent memory

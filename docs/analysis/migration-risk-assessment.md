# Functionality Risk Assessment

The hook+rules pattern is not a free swap. Several capabilities that agents provide are reduced or lost entirely. This section catalogs each risk, rates its severity, and proposes mitigations.

## Risk 1: Loss of Cross-File Analysis

**Severity: High**

Agents can read multiple files and trace relationships. Sentinel traces data flow from controller through service to data access layer. IDesign architect greps across all Manager/Engine/Accessor files to find forbidden dependencies. A PostToolUse hook fires on a **single file edit** -- it sees only the file that was just changed, not how it connects to the rest of the codebase.

**What's lost**: A hook reviewing `OrderManager.cs` won't automatically read `OrderAccessor.cs` to verify user scoping reaches the data layer. An agent would.

**Mitigation**: The hook's stderr prompt can instruct Claude to read related files: *"Review this edit and trace any data access calls to their accessor implementations to verify user scoping."* Claude in the main session has full tool access and can follow the trail. However, this adds tokens back and depends on Claude choosing to do the tracing -- it's not guaranteed the way an agent's explicit workflow is.

Additionally, PostToolUse hooks now support an `agent` handler type that can do multi-turn verification with full tool access (read files, run commands). An `agent` hook can explicitly trace data flow across files:

```json
{
  "type": "agent",
  "prompt": "Trace the data flow from the edited file through managers and accessors. Verify user scoping reaches the data access layer."
}
```

This significantly reduces the residual risk compared to a simple bash hook with a stderr prompt.

**Residual risk**: Low-Medium. The `agent` hook handler can perform systematic multi-file tracing, and the Husky pre-commit hook provides batch cross-file review at commit time. The residual gap is narrowed considerably.

## Risk 2: Loss of Persistent Agent Memory

**Severity: Medium**

Five agents have `memory: project` -- they accumulate institutional knowledge across sessions: recurring violations, codebase conventions, method length trends, coupling patterns. This memory makes reviews more targeted over time. Hooks have no memory mechanism.

**What's lost**: An agent that has reviewed 20 sessions worth of code knows "this codebase tends to violate CC-002 in the reporting module" and watches for it. A hook treats every review as the first time.

**Mitigation**: The highest-value institutional knowledge can be captured in CLAUDE.md or the rules files as project-specific notes. The main conversation's memory system (`.claude/projects/.../memory/`) can also store patterns. But neither is a direct replacement for agent-scoped memory that builds automatically.

**Residual risk**: Low-Medium. Most agent memory content is derivable from the code itself. The agents that benefit most from memory (sql-data-architect, table-storage-architect) are HYBRID -- they keep their agents for planning, where memory is most valuable.

## Risk 3: Loss of Specialized Persona and Expertise Framing

**Severity: Medium**

Each agent has a detailed system prompt that frames Claude as a specialist: *"You are an elite code cleanliness architect..."*, *"You are Sentinel, a Senior Security Research Agent..."*. This framing shapes reasoning depth, output quality, and the lens through which code is evaluated. A hook-triggered self-review against a flat rules file loses this persona framing.

**What's lost**: The difference between "check this code against rule CC-002" and "review this code as an elite cleanliness architect who believes clean code is the foundation of maintainable software." The persona prompts create more thorough, opinionated, contextualized feedback.

**Mitigation**: The hook's stderr prompt can include brief framing: *"Review as a security specialist focusing on..."* The rules files can include rationale and philosophy, not just bare rules. But this won't match the depth of a 150+ line specialized system prompt.

**Residual risk**: Medium. Review quality will likely be less nuanced. The most impactful rules (binary violations like forbidden dependencies) won't be affected. Judgment-heavy rules (class cohesion, design smells) will produce less detailed feedback.

## Risk 4: Loss of Holistic Review Across All Changes

**Severity: Medium-High**

An agent reviews all changes as a batch -- it sees the full diff and can identify patterns that emerge across multiple files. A hook fires **per edit**, reviewing each file change in isolation.

**What's lost**: If a developer adds a new Manager in file A and injects it into another Manager in file B, an agent reviewing the full diff catches the Manager->Manager dependency. Two separate hook-triggered reviews might each look fine in isolation -- file A is a valid Manager, file B's edit adds a dependency that looks like any other import.

**Mitigation**: The `/review-idesign` skill (batch review via `git diff` in `.claude/skills/review-idesign/SKILL.md`) catches these cross-file patterns on demand. Developers should run batch review skills before committing. The CLAUDE.md rules also make Claude aware proactively during authoring. The Husky pre-commit hook now provides automatic batch review of all staged files at commit time, catching cross-file patterns without manual invocation. Additionally, `agent` hooks can do limited cross-file analysis per-edit by reading related files with full tool access.

**Residual risk**: Low-Medium. The Husky pre-commit hook provides automatic holistic review at commit time. The remaining gap is that cross-file patterns introduced mid-session (before commit) rely on the `/review-idesign` command or `agent` hooks for detection.

## Risk 5: Loss of Model Control

**Severity: Low-Medium**

Agents specify their model (`model: opus` or `model: sonnet`). Some agents use opus because their review requires deep reasoning. A hook-triggered self-review runs on whatever model the current session uses.

**What's lost**: If the session runs on sonnet or haiku, security reviews and architectural compliance checks may be less thorough than they would be on opus.

**Mitigation**: This is actually a feature for most rules. The rules extracted for hooks are concrete, binary patterns (forbidden dependencies, method length thresholds, banned code patterns) that don't require opus-level reasoning. The rules that DO need deep reasoning stay with the HYBRID agents for planning sessions. For on-demand deep reviews, the skills can be run in an opus session.

**Residual risk**: Low. Binary rules don't need opus. The judgment-heavy work stays with agents.

## Risk 6: Loss of Structured Output Format

**Severity: Low**

Agents produce structured reports: sentinel outputs a finding table with ID, severity, confidence, CWE references, and remediation. Code-cleanliness outputs Issues/Warnings/Praise sections. Hook-triggered self-review produces freeform responses.

**What's lost**: Consistent, structured output that's easy to scan and track over time.

**Mitigation**: The hook's stderr prompt can request a specific format: *"Report violations as: [RULE-ID] [SEVERITY] [FILE:LINE] - description."* The skills for batch review can request the full structured format. This gets close to the original but depends on Claude following the format instruction consistently.

**Residual risk**: Low. Format can be specified in the prompt. It may be less consistent but the information content is the same.

## Risk 7: Loss of "Team Review" Dynamic

**Severity: Low-Medium**

The current architecture creates a review team where multiple specialists examine the same change from different angles simultaneously. This multi-perspective review is the core philosophy of agentFactory. With hooks, it becomes Claude reviewing against multiple flat rule lists sequentially -- the specialized lenses and creative tension between agents (e.g., code-cleanliness wanting smaller methods vs. backend-architect wanting comprehensive error handling) is gone.

**What's lost**: The emergent value of multiple independent perspectives. When sentinel and backend-architect review the same code, their findings can complement or tension each other in useful ways. With hooks, it's one Claude reviewing against concatenated rules.

**Mitigation**: This is partially inherent to the hook model and cannot be fully mitigated. However, for high-stakes reviews (pre-commit, pre-PR), the skills can invoke multiple rule sets explicitly, and the HYBRID agents can still be invoked manually for planning sessions where multi-perspective input is most valuable.

**Residual risk**: Low-Medium. The "team" value is real but hard to quantify. Most of its concrete value comes from the rules themselves (which are preserved), not from having separate agent personas.

## Risk 8: Advisory Nuance Becomes Binary Enforcement

**Severity: Low**

Code-cleanliness is explicitly described as "advisory -- not a hard block, but always has an opinion." The agent can weigh context: a 22-line method in a complex domain might get a soft warning, while a 22-line method doing simple CRUD gets a stronger push to refactor. Rules in a file tend to be binary: violates or doesn't.

**What's lost**: Contextual judgment about when a rule violation is acceptable.

**Mitigation**: Rules files can include severity levels (Info/Warning/Error) and explicit exceptions: *"CC-001: Methods over 20 lines are a red flag. Exception: complex query builders or state machines may warrant longer methods if they can't be meaningfully decomposed."* The hook prompt can say *"These are advisory guidelines, not hard blocks. Note violations but use judgment about severity in context."*

**Residual risk**: Low. Well-written rules with exceptions and severity levels preserve most of the advisory nuance.

---

## Summary: What's Actually Lost vs. Preserved

| Capability | Preserved? | Notes |
|------------|-----------|-------|
| Binary rule checking (forbidden patterns, thresholds) | **Fully preserved** | Rules file + hook + CLAUDE.md |
| Per-file review on every edit | **Fully preserved** | Hook triggers automatically |
| Cross-file data flow tracing | **Partially lost** | `agent` hook traces per-edit; skill catches it on demand; not guaranteed |
| Persistent agent memory | **Partially lost** | Key knowledge moves to CLAUDE.md; auto-accumulation lost |
| Specialized persona/expertise framing | **Partially lost** | Brief framing in hook prompt; less depth than full agent |
| Holistic multi-file review | **Available on demand** | Skills provide batch review; not automatic |
| Planning/design sessions | **Fully preserved** | HYBRID agents retained for planning |
| Deep audit workflows (sentinel 8-step) | **Fully preserved** | KEEP/HYBRID agents retained |
| Structured output format | **Mostly preserved** | Hook prompt can request format; less consistent |
| Multi-perspective "team" review | **Partially lost** | Rules preserved; creative tension between personas lost |
| Advisory nuance (soft vs. hard warnings) | **Mostly preserved** | Severity levels and exceptions in rules files |

---

## Mitigating the Losses: Three-Layer Review Architecture

The original risk assessment assumed PostToolUse hooks are the only mechanism. But Claude Code supports multiple hook types, and the losses can be substantially mitigated by using a **three-layer architecture** instead of a flat hook replacement.

### Layer 1: CLAUDE.md Rules and `.claude/rules/` Files (Proactive -- During Generation)

**What it solves**: Prevents violations before they happen. This is actually *better* than agent post-review because Claude follows the rules while writing code, not after.

The highest-value rules from each agent go into `.claude/rules/` as individual rule files with `paths:` frontmatter for conditional auto-loading (e.g., backend rules load only for `.cs` files, SQL rules for `.sql` files). The most universal rules are also inlined into CLAUDE.md. Because these are loaded into every relevant conversation, Claude applies them during code generation without any hook, any agent invocation, or any extra tokens. The violation never gets written in the first place.

**Mitigates**: Specialized persona framing (Risk 3), advisory nuance (Risk 8). The rules carry rationale and context. Claude internalizes them as part of its working instructions, which is closer to "being" the specialist than reviewing against a flat checklist after the fact.

**Token cost**: Zero incremental tokens for CLAUDE.md. Rules files with `paths:` frontmatter are loaded only when relevant.

### Layer 2: PostToolUse Hooks (Reactive -- Per Edit)

**What it solves**: Catches violations that slipped through Layer 1. Quick, cheap, per-file checks.

This is the basic hook+rules pattern described in the migration. A hook checks the file path, and if relevant, triggers a review against the rules file. Hooks support both `prompt` handler types (Claude self-reviews in the current session) and `agent` handler types (a sub-agent with full tool access performs multi-turn verification, including reading related files and running commands). Handles binary rules (forbidden patterns, thresholds, banned code) effectively.

**Mitigates**: Per-file rule checking is fully preserved. Structured output (Risk 6) can be addressed with format instructions in the hook prompt. Cross-file analysis (Risk 1) is partially addressed by `agent` handler hooks that can read related files.

**Token cost**: Low. Only fires on relevant files. Claude reads a ~2-3 KB rules file and reviews the single edit.

### Layer 3: Husky Pre-Commit Hook (Batch -- At Commit Time)

**This is the key missing piece.** Husky (`.husky/pre-commit`) fires when any developer runs `git commit`. This is the right time to do expensive, cross-file, holistic review -- the full diff is available, all changes are finalized, and the cost is paid once per commit rather than on every edit.

Husky is superior to a Claude Code hook for this purpose: it is version-controlled (all developers get it automatically), works regardless of whether Claude Code is in use, integrates with CI/CD pipelines, and is a well-established ecosystem with broad tooling support.

A Husky pre-commit hook can:

1. Run `git diff --cached --name-only` to get all staged files
2. Group files by domain (backend, frontend, data access)
3. For each relevant domain, invoke the Claude Agent SDK for AI-powered cross-file analysis, or run static linters, or both
4. Exit non-zero to block the commit if violations are found

**Mitigates**:
- **Cross-file analysis (Risk 1, severity High)**: The hook reviews the full staged diff. It can invoke the Agent SDK to trace data flow from controller to accessor, exactly like sentinel's workflow.
- **Holistic multi-file review (Risk 4, severity Medium-High)**: All changes visible at once. A new Manager in file A and a Manager injection in file B are both in the diff -- the forbidden dependency is caught.
- **Team review dynamic (Risk 7)**: The hook can trigger multiple review passes with different specialist framings or rule sets. This approximates the multi-perspective team review at commit time.
- **Structured output format (Risk 6)**: The commit-time review has room for a detailed, structured report since it runs once (not per-edit).

**Token cost**: Medium. Runs once per commit, not per edit. A development session with 20 edits but 2 commits pays for the full review twice instead of a watered-down review 20 times. Net savings are still large.

### How the Three Layers Work Together

```
Developer writes code
        |
        v
 [Layer 1: CLAUDE.md + .claude/rules/ files]
 Claude follows rules DURING generation.
 Rules files auto-load via paths: frontmatter for relevant file types.
 Most violations are prevented before they're written.
 Cost: 0 extra tokens.
        |
        v
 [Layer 2: PostToolUse Hook]
 After each Edit/Write, hook checks file path.
 prompt handler: Claude self-reviews the single edit against rules.
 agent handler: Sub-agent reads related files, traces cross-file flow.
 Catches: binary violations, threshold breaches, banned patterns,
          limited cross-file issues (via agent handler).
 Misses: holistic multi-file patterns across the full diff.
 Cost: Low (~2-5K tokens per triggered review).
        |
        v
 Developer runs `git commit`
        |
        v
 [Layer 3: Husky pre-commit hook (.husky/pre-commit)]
 Works for all developers. Version-controlled. CI/CD compatible.
 Invokes Claude Agent SDK for AI-powered cross-file analysis,
 or runs static linters, or both.
 Exits non-zero to block the commit if violations found.
 Catches: everything Layers 1-2 missed.
 Cost: Medium (~30-50K tokens, once per commit).
```

Compare this to the current agent model:

```
Current: 5 agents x 10-20K tokens x every review cycle = ~100-200K per feature
Three-layer: ~0 (CLAUDE.md) + ~20K (hooks across edits) + ~40K (Husky pre-commit) = ~60K per feature
```

The three-layer model costs less AND catches cross-file issues that per-edit hooks alone would miss.

### Husky Pre-Commit Hook Example

```bash
#!/bin/bash
# .husky/pre-commit
# Husky pre-commit hook: batch cross-file review at commit time

STAGED=$(git diff --cached --name-only)

if [ -z "$STAGED" ]; then
  exit 0
fi

BACKEND_FILES=$(echo "$STAGED" | grep -E '\.(cs|sql)$' | grep -v -E '(Tests?\.cs|/[Tt]ests?/)' || true)
FRONTEND_FILES=$(echo "$STAGED" | grep -E '\.(tsx?|css|scss)$' || true)

EXIT_CODE=0

if [ -n "$BACKEND_FILES" ]; then
  echo "Running backend review on staged files..."
  # Option 1: Invoke Claude Agent SDK for AI-powered cross-file analysis
  # claude-agent review --rules .claude/rules/backend.md,.claude/rules/idesign.md,.claude/rules/sql.md,.claude/rules/security-universal.md --files $BACKEND_FILES

  # Option 2: Run static analysis / linters
  # dotnet format --verify-no-changes $BACKEND_FILES

  # Option 3: Both
  if ! run_backend_review "$BACKEND_FILES"; then
    EXIT_CODE=1
  fi
fi

if [ -n "$FRONTEND_FILES" ]; then
  echo "Running frontend review on staged files..."
  if ! run_frontend_review "$FRONTEND_FILES"; then
    EXIT_CODE=1
  fi
fi

exit $EXIT_CODE
```

### Revised Risk Summary With Three-Layer Mitigation

| Capability | PostToolUse Only | With Three-Layer Architecture |
|------------|-----------------|-------------------------------|
| Binary rule checking | **Fully preserved** | **Fully preserved** |
| Per-file review on every edit | **Fully preserved** | **Fully preserved** |
| Cross-file data flow tracing | **Partially lost** | **Mostly preserved** -- `agent` hooks trace per-edit; Husky pre-commit traces full staged diff |
| Persistent agent memory | **Partially lost** | **Partially lost** -- same; mitigate via CLAUDE.md |
| Specialized persona framing | **Partially lost** | **Mostly preserved** -- `.claude/rules/` files with `paths:` frontmatter; Husky pre-commit uses specialist framing |
| Holistic multi-file review | **On demand only** | **Automatic at commit** -- Husky pre-commit reviews full staged diff |
| Planning/design sessions | **Fully preserved** | **Fully preserved** |
| Deep audit workflows | **Fully preserved** | **Fully preserved** |
| Structured output format | **Mostly preserved** | **Fully preserved** -- Husky pre-commit has room for structured reports |
| Multi-perspective team review | **Partially lost** | **Mostly preserved** -- Husky pre-commit runs multiple review passes |
| Advisory nuance | **Mostly preserved** | **Mostly preserved** |

The three-layer architecture resolves the two highest-severity risks (cross-file analysis and holistic review) by shifting them to commit time rather than eliminating them. `agent` hooks further reduce Risk 1 by enabling per-edit cross-file tracing. The only capability that remains genuinely degraded is **persistent agent memory**, and that's the least impactful loss since most institutional knowledge is derivable from the codebase itself.

### Recommendation

Use the three-layer architecture. It preserves nearly all agent functionality at roughly 30% of the token cost:

- **Layer 1 (CLAUDE.md + `.claude/rules/`)**: Actually *improves* on agents by preventing violations during generation instead of catching them after. Rules files with `paths:` frontmatter load conditionally for relevant file types.
- **Layer 2 (PostToolUse with `prompt` and `agent` handlers)**: Catches per-file violations cheaply and automatically. `agent` handler hooks further improve cross-file analysis by reading related files with full tool access.
- **Layer 3 (Husky pre-commit hook)**: Recovers the cross-file analysis and holistic review that per-edit hooks lose. Works for all developers, is version-controlled, and integrates with CI/CD. Can invoke the Claude Agent SDK for AI-powered review or run static linters.
- **HYBRID agents**: Still available for planning sessions and on-demand deep audits when needed

**The key architectural principle**: Automatic hooks catch the cheap, binary violations on every edit (high frequency, low cost). The Husky pre-commit hook catches cross-file and holistic issues at commit time (medium frequency, medium cost). On-demand agent invocations handle the expensive, judgment-heavy reviews when they matter most (lower frequency, justified cost). This is more efficient than running all types of analysis at full cost on every change.

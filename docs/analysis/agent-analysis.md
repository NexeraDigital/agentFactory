# Agent Analysis: Redundancy, Inefficiency, and Claude Best Practices

## Context

The agentFactory project defines 11 specialized agents organized in 3 tiers for code review, planning, and architectural governance. This analysis identifies redundancy/inefficiency, evaluates alignment with Claude best practices, and recommends better patterns from Claude's official tooling.

---

## Finding 1: Significant Redundancy Between Agents

### backend-architect vs code-cleanliness (HIGH overlap)
- **Conflicting standards**: backend-architect says methods should be "under 30 lines" (line 75), code-cleanliness says 20 lines is a "red flag" (line 18). Developers get contradictory guidance.
- **Overlapping concerns**: Both enforce SOLID principles, constructor injection, small methods, explicit dependencies, and coupling awareness.
- **Both load for every review** even when one would suffice.
- **Fix**: Delete code-cleanliness agent (migration already plans this). Harmonize method-length threshold to a single standard in rules files.

### backend-architect vs idesign-architect (MINOR overlap — KEEP BOTH)
- Both agents are kept as separate agents — they have genuinely different lenses (code quality/patterns vs. architectural methodology compliance).
- backend-architect lines 104-107 duplicate layer compliance checks that idesign-architect owns. Line 133 already defers to idesign-architect.
- **Fix**: Remove the 3 architecture compliance checklist items from backend-architect's self-verification section (lines 104-107) to eliminate the duplication. idesign-architect remains the sole owner of layer compliance.

### Three agents flag component/method size
- backend-architect: methods under 30 lines
- code-cleanliness: methods under 20 lines, components over 200 lines
- react-architect: "God components over 200 lines"
- **Fix**: Single source of truth in rules files, not scattered across 3 agents.

### data-clarifier vs modern-ui-agent (LOW overlap)
- Both recommend information hierarchy patterns for dashboards. Separation is actually well-managed by tier system (data-clarifier is Tier 3 on-demand). No action needed.

---

## Finding 2: Major Inefficiencies

### Opus overuse — 7 of 11 agents use Opus
- **Review work doesn't need Opus**: Checking if a method exceeds 20 lines, grepping for `Manager` injecting `Manager`, or scanning for missing user scoping predicates is pattern matching — Haiku or Sonnet handles this.
- **Planning work does need Opus**: Volatility decomposition, schema design, component tree design — these need strong reasoning.
- The HYBRID migration correctly separates these, but the current state wastes ~5x per-token cost on mechanical checks.
- **Agents that should be Sonnet or Haiku for review**: code-cleanliness, idesign-architect (review mode), sentinel (Scary Patterns only), sql-data-architect (anti-pattern checks), table-storage-architect (anti-pattern checks).

### "No findings" tax — ~20-30K wasted tokens per feature
- All Tier 1 agents load for every review regardless of relevance.
- backend-architect loads its 162-line prompt, reads `.tsx` files, outputs "No backend changes detected." Cost: ~10-15K input tokens for zero value.
- react-architect does the same for backend-only changes.
- **No conditional loading mechanism exists** for agents — they must fully load before they can determine irrelevance.
- **Fix**: `.claude/rules/` with `paths:` frontmatter eliminates this entirely (rules for `**/*.cs` don't load when only `.tsx` files are edited).

### Memory duplication — 70% belongs in CLAUDE.md
- 5 agents have `memory: project` but migration docs show ~70% of stored knowledge is project-wide facts (conventions, patterns, decisions) that all agents need.
- Memory loads on every invocation including "no findings" exits — wasted tokens on top of wasted prompt tokens.
- **Fix**: Move project-wide knowledge to CLAUDE.md. Keep agent memory for agent-specific observations only (<200 lines).

### Post-completion review cost: 4 agents x ~15K tokens = ~60K per review cycle
- Over 10-20 edit-review cycles per feature: 600K-1.2M tokens on reviews alone.
- **Fix**: Replace with auto-loading rules + prompt hooks (Layer 1+2 of migration plan).

### Agent prompt bloat
- **modern-ui-agent** (337 lines): ~150 lines are CSS/design token values — reference material, not instructions. Should be a separate doc file read on demand.
- **debug-investigator** (217 lines): 124 lines of phased methodology templates that Claude naturally follows when asked to debug. The high-value content is the anti-patterns section (lines 146-163) and special scenarios (lines 165-188). Could be cut to ~110 lines.
- **table-storage-architect** (228 lines): Large anti-patterns and comparison tables are reference material.

---

## Finding 3: Claude Best Practice Violations

### Not using `.claude/rules/` with `paths:` frontmatter (CRITICAL)
Claude Code's primary mechanism for domain-specific guidance is rules files with conditional loading. The current architecture ignores this entirely, loading all agent rules on every invocation. This is the single biggest missed optimization.

### Not using PostToolUse hooks (HIGH)
Claude Code has native hook infrastructure with `prompt` and `agent` handler types. A `prompt` hook runs at Haiku cost (~1-2K tokens) and auto-triggers on every edit. The current architecture requires manual agent invocation instead.

### Not using skills for batch operations (MODERATE)
Claude Code has `.claude/skills/` for on-demand workflows with `context: fork` for isolated execution. The Tier 3 on-demand agents map perfectly to skills.

### Agent prompts over-specify what Claude does naturally (MODERATE)
- debug-investigator's 7-phase methodology (124 lines) prescribes "generate hypotheses", "document findings", "be methodical" — Claude does this when simply asked to debug. The value is in the anti-patterns ("never modify code until root cause identified") and domain-specific scenarios, not the generic methodology.
- data-clarifier tells Claude to "be opinionated" and "challenge assumptions" — these are persona instructions that work with 2 lines, not 10.
- **Contrast**: sentinel's 8-step audit workflow IS valuable because it forces a specific analysis order Claude wouldn't naturally follow. idesign-architect's forbidden dependencies ARE valuable because they're non-obvious domain rules.

### Inconsistent tool restrictions (LOW)
- sentinel correctly restricts to `Glob, Grep, Read` (read-only for security auditor).
- code-cleanliness has `Bash, WebFetch, WebSearch` — why does a code reviewer need shell access and web browsing?
- 5 agents specify no tool restrictions at all, giving unrestricted access.
- **Fix**: Review-only agents should be restricted to `Read, Glob, Grep`.

### No automatic dispatch (MODERATE)
The workflow requires developers to manually invoke agents at the right stages. Claude Code's subagent system can auto-dispatch based on `description` matching. Adding dispatch instructions to CLAUDE.md would automate this.

---

## Finding 4: What Claude Provides That Would Work Better

### Official `.claude/agents/` format
The current agents live in `agents/` with custom frontmatter. Claude Code's official format at `.claude/agents/` provides:
- Automatic delegation based on `description` matching
- Built-in `model`, `tools`, `disallowedTools`, `permissionMode`, `maxTurns` fields
- `skills` preloading for domain knowledge injection
- `hooks` for lifecycle validation scoped to the agent
- `background: true` for concurrent execution
- `isolation: worktree` for safe parallel file modifications
- `memory: project` with structured storage

### The `/simplify` skill pattern
Anthropic's built-in `/simplify` skill spawns 3 parallel review agents (reuse, quality, efficiency). This is exactly the Tier 1 review team but implemented as a single skill. A `/review` skill could replace the 4-agent parallel review.

### Three-layer architecture (migration plan is correct)
The migration docs align well with Claude's official capabilities:
- **Layer 1**: `.claude/rules/` with `paths:` (replaces review role of HYBRID agents)
- **Layer 2**: PostToolUse `prompt` hooks (Haiku-level inline review)
- **Layer 3**: Skills for batch review (`/security-audit`, `/review-cleanliness`)
- **Layer 4**: Agents for planning only (generative reasoning)

### Agent Teams (experimental)
Claude Code has experimental Agent Teams for parallel independent agents with inter-agent messaging. This maps to Stage 1 Planning where 4-7 agents review a proposal simultaneously. However, it requires `CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS` and Windows support is limited.

### Anthropic's agent design principles
From "Building Effective Agents" (Anthropic research):
- "For many applications, optimizing single LLM calls with retrieval and in-context examples is usually enough."
- Start simple, add complexity only when simpler solutions fall short.
- Five patterns: Prompt Chaining > Routing > Parallelization > Orchestrator-Workers > Evaluator-Optimizer.
- The current 11-agent architecture jumps to Orchestrator-Workers complexity when Routing + Rules would handle ~70% of the work.

---

## Recommended Changes (Prioritized)

### Priority 1: Implement rules files with `paths:` frontmatter
- Extract binary/checklist rules from each HYBRID agent into `.claude/rules/`
- Use `paths:` frontmatter for domain-specific conditional loading
- Estimated savings: ~95% on review costs, ~72% overall per feature

### Priority 2: Configure PostToolUse hooks
- Add `prompt` hook for Write/Edit events that auto-reviews against loaded rules
- Cost: ~1-2K tokens per triggered check vs ~15K per agent invocation

### Priority 3: Delete code-cleanliness agent, replace with rules
- Simplest conversion, proves the pattern
- Harmonize conflicting method-length thresholds (recommend: 20 lines warning, 15 ideal)

### Priority 4: Move agents to `.claude/agents/` format
- Use official frontmatter schema (`model`, `tools`, `permissionMode`, `maxTurns`)
- Add tool restrictions: review-only agents get `tools: Read, Glob, Grep`
- Add `description` fields that enable automatic dispatch

### Priority 5: Slim agent prompts
- modern-ui-agent: Move design tokens to `docs/design-system.md`, reference on demand
- debug-investigator: Cut 7-phase templates to 15-line summary, keep anti-patterns and special scenarios (~217 to ~110 lines)
- table-storage-architect: Move comparison tables to reference docs

### Priority 6: Convert Tier 3 on-demand agents to skills
- debug-investigator > `/debug-investigate` skill with `context: fork`, `agent: debug-investigator`
- data-clarifier > `/clarify-data` skill with `context: fork`, `agent: data-clarifier`

### Priority 7: Add auto-dispatch to CLAUDE.md
```markdown
## Agent Dispatch
When planning backend work, use the backend-architect agent.
When planning frontend work, use the react-architect agent.
When encountering a bug, use the debug-investigator agent.
When designing data-heavy UI, use the data-clarifier agent.
```

### Priority 8: Clean up backend-architect / idesign-architect boundary
- Remove architecture compliance checklist items (lines 104-107) from backend-architect — idesign-architect owns layer compliance exclusively.
- Keep both agents as separate, focused lenses. No consolidation.

---

## Verification
- After implementing rules files: Edit a `.cs` file and confirm only backend/security rules load (not React/UI rules)
- After implementing hooks: Edit a file and confirm auto-review triggers
- After deleting code-cleanliness agent: Run a planning session and confirm cleanliness rules still apply
- Token comparison: Track input tokens for a typical feature before/after migration

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

#### idesign-architect: Remains HYBRID (revised from earlier CONVERT assessment)

**Previous reasoning (memory analysis)**: "Planning role is a concrete lookup table, not interactive reasoning; no memory; all rules documentable." This led to a CONVERT verdict.

**Why it reverts to HYBRID**: The project has comprehensive IDesign reference documentation in `docs/idesign-reference.md` (245 lines covering Juval Löwy's full methodology -- volatility decomposition, the layered template, interaction rules, synchronization analysis) and `docs/idesign-implementation.md` (438 lines covering coding patterns -- project structure, interface/data contract design, layer implementation patterns, DI, error handling, validation, testing, naming conventions, anti-patterns).

This changes the calculus: layer placement is NOT just a lookup table when you account for the full IDesign methodology. Deciding where a new class belongs requires understanding volatility decomposition (what changes for a customer over time vs. across customers), rejecting functional decomposition (avoiding forks and staircases), and applying the interaction rules (single manager per use case, share engines/resource access, manager-to-manager queuing). These are judgment calls informed by 683 lines of methodology, not a simple dependency direction table.

The agent becomes a **thin orchestrator** that reads these reference docs on demand:
- `docs/idesign-reference.md` for methodology (system design, volatility analysis, component design)
- `docs/idesign-implementation.md` for coding patterns (layer patterns, DI, error handling, naming)

**Revised verdict**: **HYBRID**. Review role (forbidden dependencies, Grep-based violation detection) → hooks + `docs/review-rules/idesign-rules.md`. Planning role → lean agent that reads reference docs for interactive design reasoning. Agent definition slimmed to orchestration instructions only -- all methodology content lives in the reference docs.

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
| 5 | idesign-architect | HYBRID | **HYBRID** | Reverted | Reference docs (`idesign-reference.md`, `idesign-implementation.md`) make planning role substantive; agent becomes thin orchestrator |
| 6 | azure-architect | KEEP | **KEEP** | — | Multi-step workflows, CLI, content generation |
| 7 | sql-data-architect | HYBRID | **HYBRID** | — | Schema design is generative; memory scope narrows |
| 8 | table-storage-architect | HYBRID | **HYBRID** | — | Partition key design is generative; memory scope narrows |
| 9 | data-clarifier | KEEP | **KEEP** | — | Entirely generative, on-demand |
| 10 | debug-investigator | KEEP | **KEEP** | — | On-demand workflow, requires Bash/WebSearch |
| 11 | modern-ui-agent | HYBRID | **KEEP** | Reclassified | Design sessions are generative; extract anti-patterns to hooks, design system to reference doc |

**Revised totals: 1 CONVERT, 4 KEEP, 6 HYBRID**

### Impact on Token Savings

The revised architecture still delivers substantial savings through the three-layer model:

- **Original estimate**: ~200K tokens/feature → ~60K tokens/feature (70% savings)
- **Revised estimate**: ~200K tokens/feature → ~55K tokens/feature (~72% savings)

The savings come from:
- Review role moved to hooks for all HYBRID agents (no agent invocation for per-edit checks)
- Smaller memory files across HYBRID agents (Tier 2 only, well under 200 lines)
- Lean CLAUDE.md with reference file pattern (on-demand reads vs. always-loaded)
- HYBRID agents only invoked for planning sessions, not every review cycle
- idesign-architect reads reference docs on demand instead of carrying 683 lines of methodology in its prompt

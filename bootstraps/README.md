# Agent Bootstraps

This folder holds **category-scoped, dual-tool agent bootstraps**. Each `BOOTSTRAP_*.md` is a self-contained prompt that configures a target repository to run a disciplined, evidence-based agent loop for one category of work — and it does so for **both Claude Code and OpenAI Codex** from a single shared pattern.

Every bootstrap follows the same shape:

- A shared `AGENTS.md` section (tool-neutral protocol entry point, read by both Claude and Codex).
- A category protocol at `.ai/<category>/PROTOCOL.md` (the detailed state machine and Definition of Done).
- A records template (ADR / investigation / report template) the recorder role fills in.
- **Per-role Claude agents** as Markdown + YAML-frontmatter files in `.claude/agents/`.
- **Per-role Codex agents** as standalone TOML files in `.codex/agents/` (plus `.codex/config.toml`).
- **Detection, merge, and validation rules** so the bootstrap auto-detects Claude/Codex, merges into existing config without clobbering hand-written content, and self-verifies the files it created.

Each category defines **6 logical roles** that map 1:1 onto a Claude `.md` agent and a Codex `.toml` agent.

## Categories

The family currently spans five categories, all sharing the pattern below. Pick the bootstrap that matches the kind of work; a project can adopt several.

| Category | Bootstrap file | The 6 roles | Shared protocol path | Records template path |
|---|---|---|---|---|
| Troubleshooting | `BOOTSTRAP_TROUBLESHOOTING_AGENTS.md` | evidence-collector, hypothesis-generator, skeptic, fix-designer, validator, incident-lead | `.ai/troubleshooting/PROTOCOL.md` | `docs/investigations/TEMPLATE.md` |
| Architecture | `BOOTSTRAP_ARCHITECTURE_AGENTS.md` | requirements-analyst, architecture-designer, dependency-auditor, tradeoff-skeptic, design-recorder, architecture-lead | `.ai/architecture/PROTOCOL.md` | `docs/architecture-decisions/TEMPLATE.md` |
| Design (UI/UX) | `BOOTSTRAP_DESIGN_AGENTS.md` | ux-researcher, ia-designer, visual-designer, a11y-skeptic, design-system-recorder, design-lead | `.ai/design/PROTOCOL.md` | `docs/design-decisions/TEMPLATE.md` |
| Implementation | `BOOTSTRAP_IMPLEMENTATION_AGENTS.md` | spec-analyst, implementer, test-author, code-skeptic, integration-validator, delivery-lead | `.ai/implementation/PROTOCOL.md` | `docs/implementation-notes/TEMPLATE.md` |
| Security | `BOOTSTRAP_SECURITY_AGENTS.md` | threat-modeler, vuln-hunter, exploit-skeptic, remediation-designer, security-validator, security-lead | `.ai/security/PROTOCOL.md` | `docs/security-findings/TEMPLATE.md` |

## Shared pattern

Despite different domains, every category runs the **same uniform six-role loop**:

1. **Analyst** — capture the requirement/symptom, constraints, and scope; gather evidence. No solution yet. (e.g. `requirements-analyst`, `evidence-collector`)
2. **Designer / Builder** — generate competing candidate designs or hypotheses, never collapsing to one too early. (e.g. `architecture-designer`, `hypothesis-generator`)
3. **Skeptic** — adversarially challenge the leading candidate: hidden costs, weak evidence, premature convergence, second-order effects. (e.g. `tradeoff-skeptic`, `skeptic`)
4. **Validator / Recorder** — audit/test against the rules and then durably capture the outcome in the records template. (e.g. `dependency-auditor` + `design-recorder`, `validator` + `fix-designer`)
5. **Lead** — orchestrate the state machine, delegate to the other roles, and **own the stop condition**. (e.g. `architecture-lead`, `incident-lead`)

The **lead role owns the stop condition**: work is not "done" on a plausible-looking answer. A task completes only when the category's Definition of Done is met — a decision is recorded with rationale (or a fix is implemented and validated), a specific trade-off is escalated to a human and documented, or a concrete blocker is identified with the missing information named.

Read-only roles (analyst, designer, skeptic, auditor, validator) run without write access; only the builder/recorder roles can edit files.

## How to run

1. Open the **target repository** you want to configure (not this template repo).
2. Ask your agent to follow the chosen bootstrap, e.g.:
   > Follow `BOOTSTRAP_ARCHITECTURE_AGENTS.md` and configure this repository.
3. The bootstrap **auto-detects** which tools are in use:
   - Claude Code is detected via `CLAUDE.md`, `.claude/`, `.claude/agents/`, `.claude/settings*.json`, the `claude` CLI on PATH, or the running agent self-identifying as Claude.
   - Codex is detected via `AGENTS.md`, `.codex/`, `.codex/config.toml`, `.codex/agents/`, the `codex` CLI on PATH, `CODEX_HOME`, or the running agent self-identifying as Codex.
4. It then **creates the files** for the detected tool(s) — shared files always, Claude adapters if Claude is detected, Codex adapters if Codex is detected, and both optional adapters if neither is detected. Existing hand-written content is preserved and merged, never overwritten.
5. After writing, the bootstrap runs its own **validation checklist** and emits a Bootstrap Summary (detected tools, files created/modified/preserved, model and effort choices, manual follow-ups).

No packages are installed, no secrets are added, no global (`~/.claude` / `~/.codex`) files are created, and no destructive commands are run.

## Hybrid rule-folding

Bootstraps treat project rules in two tiers:

- **Small rule tables** (the condensed tables in `.claude/rules/*.md`, typically under ~60 lines) **may be folded directly into role prompts** as a quick-reference checklist — for example the IDesign forbidden-dependency cheatsheet baked into the dependency-auditor.
- **Large authoritative documents** — the full IDesign reference/implementation guides (`docs/architecture/idesign-*.md`), `docs/design-system.md`, the security baseline, etc. — are **referenced by file path and read at runtime, never inlined** into agent prompts or `.codex/agents/*.toml`.

This keeps role prompts small while ensuring agents audit against the live source of truth: when an authoritative doc changes, agents pick up the new guidance automatically, and on any conflict the authoritative document — not a folded summary — wins.

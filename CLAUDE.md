# Agent Factory

Template repository for bootstrapping specialized AI agent architectures into new projects. Run `/bootstrap-agents` to set up the complete `.claude/` ecosystem in a target project.

## Universal Rules

- **No silent fallbacks** — failures must surface explicitly (throw, log+rethrow, meaningful error)
- **Methods ≤ 20 lines** — executable lines only; extract and compose
- **No nested ifs** — use guard clauses, pattern matching, or extraction
- **Constructor params ≤ 4** — more indicates a class with too many responsibilities
- **High cohesion, low coupling** — every field used by most methods; prefer composition

## Project Context

- @docs/architecture.md — project structure and architectural methodology
- @docs/design-system.md — visual design system reference (colors, typography, spacing)
- Domain rules auto-load from `.claude/rules/` based on file type

## Agent Dispatch

<!-- ADAPT: Remove agents not in this project's profile -->

| Work Type | Agent(s) |
|-----------|----------|
| Backend design & review | `backend-architect` |
| Frontend architecture | `react-architect` |
| IDesign compliance | `idesign-architect` |
| Security audit | `sentinel` |
| Azure infrastructure | `azure-deployment-architect` |
| SQL schema & queries | `sql-data-architect` |
| Table Storage design | `table-storage-architect` |
| UI/UX visual design | `modern-ui-agent` |
| Data-heavy UI clarity | `data-clarifier` |
| Bug investigation | `debug-investigator` |

## Skills

| Skill | Purpose |
|-------|---------|
| `/bootstrap-agents` | Bootstrap the agent architecture into a new project |
| `/debug-investigate` | Route to debug-investigator for any bug/error |
| `/clarify-data` | Route to data-clarifier for data-heavy UI design |
| `/review-cleanliness` | Batch review staged changes for code cleanliness |
| `/security-review` | Run sentinel security audit on changed files |

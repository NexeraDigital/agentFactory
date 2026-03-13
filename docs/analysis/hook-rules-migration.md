# Hook+Rules Migration Guide — Index

This guide describes how to evolve agentFactory's 11 agent definitions to use a four-layer review architecture (`.claude/rules/` auto-loading rules, PostToolUse `prompt`/`agent` hooks, Husky pre-commit hooks, on-demand skills + HYBRID agents) instead of always-on agent invocations.

**Final verdicts: 1 CONVERT, 4 KEEP, 6 HYBRID**

| Document | Content |
|----------|---------|
| [Migration Overview](migration-overview.md) | Hook+Rules pattern with `.claude/rules/` auto-loading, `prompt`/`agent` hooks, Husky pre-commit, `.claude/skills/`; decision framework, verdict tables (original + revised), totals |
| [Token Cost Analysis](migration-token-analysis.md) | 5 cost problems, per-feature estimates; auto-loading rules cost model, `prompt` hook (Haiku-level) costs, Husky/Agent SDK external costs |
| [Risk Assessment](migration-risk-assessment.md) | 8 risks, four-layer mitigation architecture, Husky pre-commit hook (not PreCommit Claude Code hook), `agent` hook handler type for cross-file analysis, revised risk summary |
| [CLAUDE.md Architecture](migration-claude-md-architecture.md) | `.claude/rules/` with `paths:` frontmatter replaces reference table, `@path` imports, lean index pattern, template, token impact |
| [Agent Memory Analysis](migration-agent-memory.md) | Current memory design, 5 problems, `.claude/agents/` official format, official memory scopes (user/project/local), per-agent migration actions |
| [Implementation Details](migration-implementation.md) | What to change per agent (CONVERT/HYBRID/KEEP), `.claude/rules/` with `paths:` frontmatter, `.claude/settings.json` with `prompt`/`agent` hooks, `.claude/skills/`, Husky setup, migration checklist |

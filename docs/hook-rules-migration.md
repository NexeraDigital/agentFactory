# Hook+Rules Migration Guide — Index

This guide describes how to evolve agentFactory's 11 agent definitions to use a three-layer review architecture (CLAUDE.md rules, PostToolUse hooks, PreCommit hooks) instead of always-on agent invocations.

**Final verdicts: 1 CONVERT, 4 KEEP, 6 HYBRID**

| Document | Content |
|----------|---------|
| [Migration Overview](migration-overview.md) | Hook+Rules pattern, decision framework, verdict tables (original + revised), totals |
| [Token Cost Analysis](migration-token-analysis.md) | 5 cost problems, per-feature estimates, hook+rules savings breakdown |
| [Risk Assessment](migration-risk-assessment.md) | 8 risks, three-layer mitigation architecture, PreCommit hook example, revised risk summary |
| [CLAUDE.md Architecture](migration-claude-md-architecture.md) | Lean index pattern, reference file structure, template, token impact |
| [Agent Memory Analysis](migration-agent-memory.md) | Current memory design, 5 problems, two-tier recommendation, per-agent migration actions |
| [Implementation Details](migration-implementation.md) | What to change per agent (CONVERT/HYBRID/KEEP), hook architecture, hooks.json, migration checklist |

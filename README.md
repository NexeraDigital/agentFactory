# agentFactory

A template repository that bootstraps specialized AI agent architectures into software projects using [Claude Code](https://claude.com/claude-code). Provides 10 opinionated agents, 9 rules files, a pre-commit hook, and 4 skills — all customized to your project's tech stack automatically.

## Getting Started

### Option 1: New Project (GitHub Template)

1. Click **"Use this template"** on GitHub to create a new repository
2. Open the new repo in Claude Code
3. Run `/bootstrap-agents`
4. Answer 2-4 questions about your tech stack
5. The skill customizes all templates for your project

### Option 2: Existing Project (GitHub MCP)

If you have the [GitHub MCP server](https://github.com/modelcontextprotocol/servers/tree/main/src/github) configured in Claude Code, just ask:

```
Read the agentFactory repo at github.com/NexeraDigital/agentFactory
and bootstrap the agent architecture into this project
```

Claude fetches all templates via the GitHub MCP server and writes customized versions to your project.

### Option 3: Existing Project (Manual Skill Install)

Copy just the bootstrap skill, then run it:

```bash
mkdir -p .claude/skills/bootstrap-agents
curl -s https://raw.githubusercontent.com/NexeraDigital/agentFactory/main/.claude/skills/bootstrap-agents/SKILL.md \
  > .claude/skills/bootstrap-agents/SKILL.md
```

Then in Claude Code:

```
/bootstrap-agents
```

The skill fetches remaining templates from GitHub and walks you through setup.

### Prerequisites

- [Claude Code](https://claude.com/claude-code) CLI
- For Option 2/3: [GitHub MCP server](https://github.com/modelcontextprotocol/servers/tree/main/src/github) configured in Claude Code

---

## What You Get

A three-layer quality enforcement architecture:

| Layer | What | How |
|-------|------|-----|
| **1. Rules** | 9 rules files in `.claude/rules/` | Auto-load by file type, enforce patterns continuously |
| **2. Pre-commit** | Pre-commit hook (Husky or git hooks) | Batch-reviews staged changes, blocks commit on CRITICAL violations |
| **3. Skills + Agents** | `/debug-investigate`, `/clarify-data`, `/review-cleanliness` + 10 specialized agents | On-demand batch review, specialist routing, planning sessions, design guidance |

### Agents

| Agent | Role | Model |
|-------|------|-------|
| `backend-architect` | Code design, SOLID, GoF patterns, error handling | Opus |
| `react-architect` | Component architecture, state management, TypeScript | Opus |
| `sentinel` | Full-stack security audit, OWASP Top 10, auth/authz | Opus |
| `idesign-architect` | IDesign layer compliance (Manager/Engine/Accessor) | Opus |
| `sql-data-architect` | Schema design, migrations, query performance | Opus |
| `table-storage-architect` | Partition key design, denormalization, NoSQL patterns | Opus |
| `modern-ui-agent` | Visual design, design systems, UI polish | Default |
| `azure-deployment-architect` | Azure infrastructure, Bicep, CI/CD, Key Vault | Sonnet |
| `data-clarifier` | Data-heavy UI clarity, progressive disclosure | Opus |
| `debug-investigator` | Scientific debugging, root cause analysis | Opus |

## Project Profiles

Not every project needs all 10 agents. The bootstrap detects your tech stack and recommends a profile:

| Profile | Agents | When |
|---------|--------|------|
| **Full-Stack** | All 10 | Backend API + frontend SPA + cloud |
| **API-Only** | 7 | Backend API, no frontend |
| **Frontend-Only** | 5 | SPA/frontend, no backend |
| **Minimal** | 2 | Scripts, utilities, small projects |

## How It Works

### `/bootstrap-agents` Procedure

1. **Read templates** — Fetches agent, rules, skill, and hook templates from agentFactory
2. **Discover project** — Auto-detects backend, frontend, infrastructure, data persistence, testing
3. **Ask questions** — Only what it can't detect (2-4 questions)
4. **Create rules** — Customizes `.claude/rules/` with project-specific file globs
5. **Create pre-commit hook** — Installs via Husky (Node.js) or `.git/hooks/` with project-specific patterns
6. **Create agents** — Customizes `.claude/agents/` with project context injected
7. **Create skills** — Copies `/debug-investigate`, `/clarify-data`, `/review-cleanliness`
8. **Create memory & docs** — Agent memory directories, CLAUDE.md, reference docs
9. **Summary** — Lists everything created with verification

### Template Customization

Every template contains `<!-- ADAPT -->` markers where project-specific content is injected. The bootstrap replaces these with real values based on auto-detection and your answers. The core methodology (80-90% of each file) is preserved exactly — only the marked sections are customized.

## Repository Structure

```
CLAUDE.md                                  Template (~35 lines)
agent-architecture.md                      Methodology (10-agent roster, workflow stages)
.claude/
  rules/                                   9 rules templates
  agents/                                  10 agent templates
  skills/
    bootstrap-agents/SKILL.md              The bootstrapper itself
    debug-investigate/SKILL.md             Routes to debug-investigator
    clarify-data/SKILL.md                  Routes to data-clarifier
    review-cleanliness/SKILL.md            Batch code cleanliness review
  agent-memory/                            4 memory directories
templates/
  hooks/
    pre-commit                             Pre-commit hook template (Layer 2)
docs/
  architecture.md                          Project architecture template
  design-system.md                         Design system reference template
  architecture/                            IDesign reference docs
  analysis/                                Migration analysis docs
```

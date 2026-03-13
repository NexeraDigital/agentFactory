---
name: bootstrap-agents
description: "Bootstrap the complete agent architecture into a new project. Detects tech stack, creates rules, agents, skills, memory, and CLAUDE.md."
user-invocable: true
allowed-tools: Read, Write, Edit, Glob, Grep, Bash, WebFetch, mcp__github-server__get_file_contents
---

# Agent Architecture Bootstrap

You are about to set up a complete team of specialized AI agents for this project. This bootstrap detects the project's technology stack, asks clarifying questions, and creates the full `.claude/` ecosystem — rules, hooks, agents, skills, memory, and CLAUDE.md.

## Source Materials

The agent methodology and templates live in the **agentFactory** GitHub repository: `NexeraDigital/agentFactory`

### Template Manifest

These are the files to fetch from the agentFactory repo:

**Methodology:**
- `agent-architecture.md`

**Agents** (`.claude/agents/`):
- `backend-architect.md`, `react-architect.md`, `sentinel.md`, `idesign-architect.md`
- `sql-data-architect.md`, `table-storage-architect.md`, `modern-ui-agent.md`
- `azure-architect.md`, `data-clarifier.md`, `debug-investigator.md`

**Rules** (`.claude/rules/`):
- `universal.md`, `security-universal.md`, `backend.md`, `idesign.md`
- `react.md`, `sql.md`, `table-storage.md`, `ui-design.md`, `code-cleanliness.md`

**Skills** (`.claude/skills/`):
- `debug-investigate/SKILL.md`, `clarify-data/SKILL.md`, `review-cleanliness/SKILL.md`

**Hooks:**
- `templates/hooks/pre-commit`

**Other:**
- `CLAUDE.md`, `docs/design-system.md`, `docs/architecture.md`

### How to Fetch Templates

**If running from a local clone of agentFactory:** Read files directly using the `Read` tool.

**If running in a target project (no local templates):** Fetch templates from GitHub using the `mcp__github-server__get_file_contents` tool:
```
owner: "NexeraDigital"
repo: "agentFactory"
path: "<file path from manifest above>"
```

Fetch templates in batches — start with `agent-architecture.md` to understand the methodology, then fetch agent/rules/skill templates as needed during the creation phases.

**Read the methodology and all relevant templates before proceeding.**

## Bootstrap Procedure

Execute these phases in order. Do not skip phases. Do not make assumptions — verify by reading actual files.

---

### Phase 1: Read Templates

1. Fetch and read `agent-architecture.md` in full
2. Fetch templates for agents, rules, and skills from the manifest above
3. Fetch `CLAUDE.md`, `docs/design-system.md`, `docs/architecture.md`
4. Internalize the agent roles, rules structure, workflow stages, and adaptation requirements

---

### Phase 2: Discover Target Project

Auto-detect as much as possible by reading project files. Gather evidence before asking the user anything.

#### 2a: Project Root & Structure
- Look for `CLAUDE.md` at the project root — read it entirely if it exists
- Run `ls` on the project root to understand the top-level structure
- Look for solution files (`.slnx`, `.sln`), `package.json`, `Cargo.toml`, `go.mod`, `pyproject.toml`, etc.
- Look for `specs/`, `docs/`, `requirements/` directories

#### 2b: Backend Detection
- Search for backend project files (`.csproj`, `pom.xml`, `build.gradle`, `Gemfile`, etc.)
- Read the main backend project file for language, framework, version, key dependencies
- Look for entry points (`Program.cs`, `Startup.cs`, `app.py`, `main.go`, `index.ts`)
- Look for architectural patterns (Managers/Engines/Accessors = IDesign, Controllers/Services/Repositories = layered)

#### 2c: Frontend Detection
- Search for frontend project files (`package.json` in web/frontend directories)
- Determine framework, build tool, styling approach, state management, routing, HTTP client
- Examine `src/` structure for folder organization

#### 2d: Infrastructure & Deployment Detection
- Look for `infra/`, `bicep/`, `terraform/`, `cdk/`, `.github/workflows/`, `Dockerfile`, `docker-compose.yml`
- Check for cloud-specific config files

#### 2e: Data Persistence Detection
- Look for relational DB evidence: `DbContext` subclasses, `.sql` migrations, `Microsoft.EntityFrameworkCore` packages
- Look for Table Storage evidence: `TableClient` usage, `Azure.Data.Tables` packages
- Note: Confirm explicitly in Phase 3 — don't auto-decide

#### 2f: Testing Detection
- Look for test projects or directories (`tests/`, `__tests__/`, `spec/`, `test/`)
- Identify test frameworks

#### 2g: Existing Setup
- Check for existing `.claude/agents/`, `.claude/rules/`, `.claude/skills/`
- Check for `.claude/agent-memory/`

---

### Phase 3: Present Findings & Ask Clarifying Questions

Present discovery summary, then ask ONLY about unknowns:

```
## Project Discovery Summary

### Detected Tech Stack
- **Backend:** [what you found, or "None detected"]
- **Frontend:** [what you found, or "None detected"]
- **Infrastructure:** [what you found, or "None detected"]
- **Testing:** [what you found, or "None detected"]

### Detected Architecture
- **Backend pattern:** [IDesign / Layered / Clean / Hexagonal / None detected]
- **Frontend pattern:** [Feature-based / Page-based / Flat / None detected]

### Detected Data Persistence
- **Relational (SQL):** [Evidence found / None detected]
- **Azure Table Storage:** [Evidence found / None detected]

### Recommended Profile
- **Profile:** [Full-Stack / API-Only / Frontend-Only / Minimal]
- **Agents:** [list]
- **Rules:** [list]
- **Skills:** [list]

### Existing Setup
- **CLAUDE.md:** [Exists / Does not exist]
- **.claude/:** [What exists]
```

Typical questions (only ask what's unknown):
- **Data persistence (ALWAYS ASK):** Which model(s)? SQL, Table Storage, Both, Other?
- **Cloud provider:** Only if no infra files detected
- **Architectural paradigm:** Only if ambiguous
- **Auth mechanism:** Only if not detectable
- **Existing agents:** Confirm update/replace/leave if agents already exist
- **Profile confirmation:** Confirm recommended agent + rules profile

**Do NOT ask about things already detectable.** Target 2-4 questions, not a survey.

---

### Phase 4: Create Rules Files

For each rules file in the confirmed profile:

1. Fetch the template from agentFactory `.claude/rules/<rule-name>.md`
2. Replace `# ADAPT` paths in frontmatter with project-specific `paths:` values:
   - React detected? → `react.md` paths: `["**/*.tsx", "**/*.ts"]`
   - C# detected? → `backend.md` paths: `["**/*.cs"]`
   - Python detected? → add equivalent paths for `**/*.py`
3. Replace `<!-- ADAPT -->` content sections with project-specific patterns
4. Write to target project's `.claude/rules/`

**Rules profile mapping:**

| Project Profile | Rules to Include |
|----------------|-----------------|
| Full-Stack | All 9 rules files |
| API-Only | universal, security-universal, backend, idesign, sql/table-storage, code-cleanliness |
| Frontend-Only | universal, security-universal, react, ui-design, code-cleanliness |
| Minimal | universal, security-universal, code-cleanliness |

---

### Phase 5: Create Pre-Commit Hook

Fetch `templates/hooks/pre-commit` from agentFactory.

**If Node.js project** (`package.json` exists at project root):

1. Install Husky: `npm install --save-dev husky && npx husky init`
2. Write the pre-commit template to `.husky/pre-commit`
3. Replace `<!-- ADAPT -->` markers with detected file patterns:
   - Backend patterns based on detected language (`.cs`, `.py`, `.go`, `.java`, etc.)
   - Frontend patterns based on detected framework (`.tsx?`, `.vue`, `.svelte`, etc.)
   - Exclude patterns for test directories
4. Uncomment relevant static linter sections (dotnet format, eslint, etc.)
5. `chmod +x .husky/pre-commit`

**If not Node.js:**

1. Write the pre-commit template to `.git/hooks/pre-commit`
2. Replace `<!-- ADAPT -->` markers with detected file patterns
3. Uncomment relevant static linter sections
4. `chmod +x .git/hooks/pre-commit`

---

### Phase 6: Create Agent Files

For each agent in the confirmed profile:

1. Fetch the template from agentFactory `.claude/agents/<agent-name>.md`
2. Replace ALL `<!-- ADAPT -->` sections with project-specific content
3. Remove `<!-- ADAPT -->` comment markers entirely — final files should have no markers
4. Write to target `.claude/agents/<agent-name>.md`

**Customization rules:**
- Frontmatter (name, description, tools, model, memory) kept as-is unless specific reason to change
- Core methodology (80-90% of each file) preserved exactly — do not edit, summarize, or "improve"
- Project-Specific Context section fully replaced with real project details
- Persistent Agent Memory section gets correct path for this project
- For `memory: project` agents, memory path should be `.claude/agent-memory/<agent-name>/`

**Special agents:**
- `idesign-architect`: Keep if IDesign; rename/adapt for other paradigms
- Data agents: Include only for confirmed persistence models
- `azure-architect`: Keep for Azure; duplicate/adapt for other cloud providers

---

### Phase 7: Create Skills

For each skill in the confirmed profile:

1. Fetch skill template and write to target project's `.claude/skills/`
2. Skills reference agents by name — if the agent wasn't created, skip the skill

**Skill profile mapping:**

| Project Profile | Skills to Include |
|----------------|-----------------|
| Full-Stack | debug-investigate, clarify-data, review-cleanliness |
| API-Only | debug-investigate, review-cleanliness |
| Frontend-Only | debug-investigate, clarify-data, review-cleanliness |
| Minimal | debug-investigate, review-cleanliness |

---

### Phase 8: Create Memory, CLAUDE.md, Reference Docs

**Agent Memory:**
For each agent with `memory: project` in frontmatter:
1. Create `.claude/agent-memory/<agent-name>/`
2. Create `MEMORY.md`:
   ```
   # <Agent Name> Memory

   This file is loaded into the agent's system prompt. Keep it under 200 lines.

   Agent-specific observations only — project-wide knowledge goes in CLAUDE.md.
   ```

**CLAUDE.md:**
Fetch CLAUDE.md template, then create or update for target project:
- Replace universal rules with project-specific emphasis
- Remove `<!-- ADAPT -->` markers for agents/rules not in the profile
- Add project-specific context from discovery
- Keep under ~35 lines

**Reference Docs:**
- `docs/design-system.md` — only if frontend agents included; fetch template, customize with project's design tokens
- `docs/architecture.md` — always; fetch template, fill with detected project structure

---

### Phase 9: Summary & Verification

Present final summary:

```
## Bootstrap Complete

### Rules Created ([N] total)
| Rule | File | Loading |
|------|------|---------|
| [name] | .claude/rules/[file] | [Always / Conditional] |

### Agents Created ([N] total)
| Agent | File | Memory |
|-------|------|--------|
| [name] | .claude/agents/[file] | [Yes/No] |

### Skills Created ([N] total)
| Skill | File | Agent |
|-------|------|-------|
| [name] | .claude/skills/[dir]/SKILL.md | [agent name] |

### Hooks
- Pre-commit hook installed at [.husky/pre-commit | .git/hooks/pre-commit]

### CLAUDE.md
- [Created / Updated] with agent dispatch and universal rules

### Architecture
- Layer 1: Rules auto-load from .claude/rules/ based on file type
- Layer 2: Pre-commit hook batch-reviews staged changes before commit
- Layer 3: Skills available for batch review and on-demand specialists
- Layer 4: Agents available for planning and design sessions

### Next Steps
- [Any manual steps needed]
- [Any agents that need additional context]
```

---

## Important Rules

1. **Read before writing.** Always fetch templates and read project files before generating anything.
2. **Preserve methodology.** Core agent methodology is battle-tested. Do not edit, summarize, condense, or "improve" it. Only customize marked sections.
3. **Don't over-ask.** Auto-detect everything you can. Target 2-4 questions.
4. **Don't over-create.** Only create artifacts appropriate for the project profile.
5. **No placeholders in output.** Every `<!-- ADAPT -->` marker must be replaced or removed. Final files must be production-ready.
6. **Respect existing work.** Confirm with user before overwriting existing `.claude/` content.
7. **Be explicit about what you created.** Phase 9 summary lists every file created or modified.

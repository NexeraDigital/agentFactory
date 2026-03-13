---
name: bootstrap
description: "Bootstrap the complete agent architecture into a new project. Detects tech stack, creates rules, agents, hooks, skills, memory, and CLAUDE.md."
user-invocable: true
allowed-tools: Read, Write, Edit, Glob, Grep, Bash
---

# Agent Architecture Bootstrap

You are about to set up a complete team of specialized AI agents for this project. This bootstrap detects the project's technology stack, asks clarifying questions, and creates the full `.claude/` ecosystem — rules, hooks, agents, skills, memory, and CLAUDE.md.

## Source Materials

The agent methodology and templates live in the agentFactory repository:

- `agent-architecture.md` — The methodology document defining roles, workflow stages, RACI chart, and project profiles
- `.claude/agents/` — Agent template files with `<!-- ADAPT -->` markers
- `.claude/rules/` — Rules template files with `<!-- ADAPT -->` markers
- `.claude/skills/` — Skill template files
- `.claude/settings.json` — Hook configuration template
- `CLAUDE.md` — CLAUDE.md template
- `docs/design-system.md` — Design system reference template
- `docs/architecture.md` — Architecture overview template

**Read ALL templates before proceeding.** You need the full methodology to make good customization decisions.

## Bootstrap Procedure

Execute these phases in order. Do not skip phases. Do not make assumptions — verify by reading actual files.

---

### Phase 1: Read Templates

1. Read `agent-architecture.md` in full
2. Read every file in `.claude/agents/`, `.claude/rules/`, `.claude/skills/`
3. Read `CLAUDE.md`, `.claude/settings.json`, `docs/design-system.md`, `docs/architecture.md`
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
- Check for existing `.claude/agents/`, `.claude/rules/`, `.claude/skills/`, `.claude/settings.json`
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

1. Read the template from the agentFactory `.claude/rules/<rule-name>.md`
2. Replace `<!-- ADAPT -->` paths with project-specific globs:
   - React detected? → `react.md` globs: `**/*.tsx`, `**/*.ts`
   - C# detected? → `code-cleanliness.md` globs: `**/*.cs`, `**/*.tsx`, `**/*.ts`
   - Python detected? → `code-cleanliness.md` globs: `**/*.py`
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

### Phase 5: Create Settings & Hooks

Copy `.claude/settings.json` hook template to target project. No customization needed — the PostToolUse prompt hook works generically against whatever rules are loaded.

Preserve any existing `.claude/settings.local.json` (user-specific permissions).

---

### Phase 6: Create Agent Files

For each agent in the confirmed profile:

1. Read the template from agentFactory `.claude/agents/<agent-name>.md`
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

1. Copy skill template to target project's `.claude/skills/`
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
Create or update from template:
- Replace universal rules with project-specific emphasis
- Remove `<!-- ADAPT -->` markers for agents/rules not in the profile
- Add project-specific context from discovery
- Keep under ~35 lines

**Reference Docs:**
- `docs/design-system.md` — only if frontend agents included; customize with project's design tokens
- `docs/architecture.md` — always; fill with detected project structure

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
- PostToolUse prompt hook configured in .claude/settings.json

### CLAUDE.md
- [Created / Updated] with agent dispatch and universal rules

### Architecture
- Layer 1: Rules auto-load from .claude/rules/ based on file type
- Layer 2: PostToolUse hook auto-reviews edits against loaded rules
- Layer 3: Skills available for batch review and on-demand specialists
- Layer 4: Agents available for planning and design sessions

### Next Steps
- [Any manual steps needed]
- [Any agents that need additional context]
```

---

## Important Rules

1. **Read before writing.** Always read templates and project files before generating anything.
2. **Preserve methodology.** Core agent methodology is battle-tested. Do not edit, summarize, condense, or "improve" it. Only customize marked sections.
3. **Don't over-ask.** Auto-detect everything you can. Target 2-4 questions.
4. **Don't over-create.** Only create artifacts appropriate for the project profile.
5. **No placeholders in output.** Every `<!-- ADAPT -->` marker must be replaced or removed. Final files must be production-ready.
6. **Respect existing work.** Confirm with user before overwriting existing `.claude/` content.
7. **Be explicit about what you created.** Phase 9 summary lists every file created or modified.

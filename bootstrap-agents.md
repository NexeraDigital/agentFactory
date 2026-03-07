# Agent Architecture Bootstrap

You are about to set up a team of specialized AI agents for this project. This bootstrap will auto-detect the project's technology stack, ask clarifying questions for anything it can't infer, and then create fully customized agent definitions.

## Source Materials

The agent methodology and templates live at `C:\github\nexera\templates\`:

- `agent-architecture.md` — The methodology document defining roles, workflow stages, RACI chart, and project profiles
- `agents/` — Full agent template files with `<!-- ADAPT -->` markers for project-specific customization

**Read ALL of these files before proceeding.** You need to understand the full methodology to make good customization decisions.

## Bootstrap Procedure

Execute these phases in order. Do not skip phases. Do not make assumptions — verify by reading actual files.

---

### Phase 1: Read the Methodology

1. Read `C:\github\nexera\templates\agent-architecture.md` in full
2. Read every file in `C:\github\nexera\templates\agents\`
3. Internalize the agent roles, workflow stages, and adaptation requirements

---

### Phase 2: Discover the Project

Auto-detect as much as possible by reading project files. Gather evidence before asking the user anything.

#### 2a: Project Root & Structure
- Look for `CLAUDE.md` at the project root — if it exists, read it entirely. This is the most authoritative source of project context.
- Run `ls` on the project root to understand the top-level structure
- Look for solution files (`.slnx`, `.sln`), `package.json`, `Cargo.toml`, `go.mod`, `pyproject.toml`, etc. to identify the tech stack
- Look for `specs/`, `docs/`, `requirements/`, or similar directories that describe the project domain

#### 2b: Backend Detection
- Search for backend project files (`.csproj`, `pom.xml`, `build.gradle`, `Gemfile`, etc.)
- Read the main backend project file to determine:
  - Language and framework (e.g., .NET 10 / ASP.NET Core, Node.js / Express, Python / FastAPI)
  - Target framework version
  - Key dependencies
- Look for `Program.cs`, `Startup.cs`, `app.py`, `main.go`, `index.ts`, etc. to understand the entry point and architecture
- Look for directory patterns that suggest an architectural methodology (Managers/Engines/Accessors = IDesign, Controllers/Services/Repositories = traditional layered, etc.)

#### 2c: Frontend Detection
- Search for frontend project files (`package.json` in a web/frontend directory)
- If found, read it to determine:
  - Framework (React, Vue, Angular, Svelte, etc.) and version
  - Build tool (Vite, Webpack, Next.js, etc.)
  - Styling approach (Tailwind, CSS Modules, styled-components, etc.)
  - State management libraries
  - Routing libraries
  - HTTP client
- Look at the `src/` structure to understand the current folder organization

#### 2d: Infrastructure & Deployment Detection
- Look for `infra/`, `bicep/`, `terraform/`, `cdk/`, `.github/workflows/`, `Dockerfile`, `docker-compose.yml`
- Look for cloud-specific config files (`appsettings.json` with Azure sections, `serverless.yml`, AWS CDK files)
- Check for CI/CD pipeline files

#### 2e: Data Persistence Detection
- Look for evidence of relational databases: `DbContext` subclasses, `.sql` migration files, `Microsoft.EntityFrameworkCore` packages, connection strings containing `Server=` or `Data Source=`
- Look for evidence of Azure Table Storage: `TableClient` usage, `Azure.Data.Tables` package references, connection strings containing `TableEndpoint=` or `UseDevelopmentStorage=true`
- Note: Do NOT auto-decide the persistence model based on detection alone. This will be explicitly confirmed with the user in Phase 3.

#### 2f: Testing Detection
- Look for test projects or directories (`tests/`, `__tests__/`, `spec/`, `test/`)
- Identify test frameworks from project files

#### 2g: Existing Agent Setup
- Check if `.claude/agents/` already exists — if so, read what's there to avoid overwriting existing work
- Check if `.claude/agent-memory/` exists

---

### Phase 3: Present Findings & Ask Clarifying Questions

Present what you discovered in a structured summary, then ask ONLY about things you couldn't determine from the codebase. Use the AskUserQuestion tool for structured questions.

#### Always present:
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
- **Relational (SQL):** [Evidence found / None detected] — [describe evidence if found]
- **Azure Table Storage:** [Evidence found / None detected] — [describe evidence if found]

### Recommended Agent Profile
- **Profile:** [Full-Stack / API-Only / Frontend-Only / Minimal]
- **Agents to create:** [list]
- **Agents to skip:** [list and why]

### Existing Setup
- **CLAUDE.md:** [Exists / Does not exist]
- **.claude/agents/:** [Exists with N agents / Does not exist]
```

#### Then ask about things you couldn't detect:

Typical questions (only ask what's actually unknown):
- **Project domain:** "What does this application do? What are the key business domains?" (Only if not clear from CLAUDE.md or specs/)
- **Cloud provider:** "Which cloud provider(s) will this deploy to?" (Only if no infra files detected)
- **Data persistence model (ALWAYS ASK):** "Which data persistence model(s) does this project use? (a) SQL Server / Azure SQL Database, (b) Azure Table Storage / Cosmos DB Table API, (c) Both, (d) Neither / other." This determines whether to include the SQL Data Architect, Table Storage Architect, both, or neither. Even if evidence is detected in Phase 2e, confirm explicitly — the user may be planning to add or remove a persistence layer.
- **Architectural paradigm:** "I detected [pattern]. Is this intentional, or do you prefer a different architectural methodology?" (Only if ambiguous)
- **Auth mechanism:** "What authentication approach are you using?" (Only if not detectable from code)
- **Existing agents:** "I found existing agents in .claude/agents/. Should I update them, replace them, or leave them alone?" (Only if agents already exist)
- **Agent profile confirmation:** "Based on the project, I recommend the [X] profile with these agents: [list]. Does this look right, or should I add/remove any?"

**Do NOT ask about things you already know from reading the codebase.** The user should only need to answer 2-4 questions, not fill out a survey.

---

### Phase 4: Create the Agent Files

For each agent in the confirmed profile:

1. Read the template from `C:\github\nexera\templates\agents/<agent-name>.md`
2. Replace ALL `<!-- ADAPT -->` sections with project-specific content based on your discovery and the user's answers
3. Remove the HTML comment markers entirely — the final files should have no `<!-- ADAPT -->` comments
4. Write the customized file to `.claude/agents/<agent-name>.md`

**Customization rules:**
- The **frontmatter** (name, description, tools, model, color) should be kept as-is from the template unless there's a specific reason to change it
- The **core methodology** (the 80-90% that's project-agnostic) must be preserved exactly — do not edit, summarize, or "improve" it
- The **Project-Specific Context** section gets fully replaced with real project details
- The **Persistent Agent Memory** section gets the correct path for this project
- For agents with `memory: project`, the memory path should be `.claude/agent-memory/<agent-name>/`

**For the idesign-architect specifically:**
- If the project uses IDesign, keep it named `idesign-architect`
- If the project uses a different paradigm, rename and adapt accordingly
- The IDesign rules (forbidden dependencies, layer definitions) are Nexera's standard — preserve them for IDesign projects

**For the data persistence agents specifically:**
- Include `sql-data-architect` only if the user confirmed SQL Server / Azure SQL Database
- Include `table-storage-architect` only if the user confirmed Azure Table Storage / Cosmos DB Table API
- If both, include both — they are independent agents with non-overlapping concerns
- If neither, skip both agents entirely

**For the azure-architect specifically:**
- If deploying to Azure, keep it named `azure-architect` or `azure-deployment-architect`
- If deploying to AWS/GCP, duplicate and adapt the template for that provider
- For multi-cloud, create one agent per provider

---

### Phase 5: Create Agent Memory Directories

For each agent that has `memory: project` in its frontmatter:

1. Create the directory `.claude/agent-memory/<agent-name>/`
2. Create an empty `MEMORY.md` file in that directory with a single header line:
   ```
   # <Agent Name> Memory

   This file is loaded into the agent's system prompt. Keep it under 200 lines.
   ```

---

### Phase 6: Update CLAUDE.md

#### If CLAUDE.md exists:
- Read it fully
- Check if an "Agent Workflow" section already exists
  - If yes, update it to reflect the agents that were created
  - If no, append the Agent Workflow section before the last major section (typically "Important Context" or similar)

#### If CLAUDE.md does not exist:
- Create a foundational CLAUDE.md with:
  - Project name and description (from what you discovered)
  - Tech stack summary
  - Repository layout
  - Build & run commands (if detectable)
  - Agent Workflow section
  - Important context

#### The Agent Workflow section should follow this structure:

```markdown
## Agent Workflow

Specialized agents in `.claude/agents/` are invoked at specific workflow stages:

### Planning Sessions
[List agents that participate in planning, with brief role descriptions]

### During Code Authoring
[List agents active during authoring, mapped to work type]

### Post-Completion Code Review (MANDATORY)
[List the review team agents and their focus areas]

### On-Demand Agents
[List on-demand agents and their triggers]
```

Customize the content based on which agents were actually created (don't list agents that weren't included in this project's profile).

---

### Phase 7: Summary & Verification

Present a final summary of everything that was created:

```
## Bootstrap Complete

### Agents Created ([N] total)
| Agent | File | Memory |
|-------|------|--------|
| [name] | .claude/agents/[file] | [Yes/No] |
...

### CLAUDE.md
- [Created / Updated] with Agent Workflow section

### Agent Workflow
- **Planning team:** [list]
- **Review team:** [list]
- **On-demand:** [list]

### Next Steps
- [Any manual steps the user needs to take]
- [Any agents that need additional context the bootstrap couldn't determine]
```

---

## Important Rules

1. **Read before writing.** Always read the template files and project files before generating anything. Never guess at project structure.
2. **Preserve methodology.** The core agent methodology in each template is battle-tested. Do not edit, summarize, condense, or "improve" it. Only customize the marked sections.
3. **Don't over-ask.** Auto-detect everything you can. The user should answer 2-4 questions, not 20.
4. **Don't over-create.** Only create agents appropriate for the project profile. A simple API doesn't need a Data Clarifier.
5. **No placeholders in output.** Every `<!-- ADAPT -->` marker must be replaced with real content or removed entirely. The final agent files should be production-ready.
6. **Respect existing work.** If `.claude/agents/` already has agents, confirm with the user before overwriting. If `CLAUDE.md` exists, append — don't replace.
7. **Be explicit about what you created.** The summary in Phase 7 should list every file created or modified so the user can verify.

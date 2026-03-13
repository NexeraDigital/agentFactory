# Migration Implementation Details

## What to Change: CONVERT Agents

### code-cleanliness (FULL CONVERSION)

**Delete**: `.claude/agents/code-cleanliness.md`

**Create**: `.claude/rules/code-cleanliness.md` with these rules:

```yaml
---
paths:
  - "**/*.cs"
  - "**/*.tsx"
  - "**/*.ts"
---
```

| ID | Severity | Rule | Bad Example | Good Example |
|----|----------|------|-------------|--------------|
| CC-001 | Warning/Error | Methods over 15 lines flagged; over 20 lines is a red flag. Count executable lines only (skip blanks, braces, comments). | A 25-line method doing validation + mapping + persistence | Three 8-line methods: `Validate()`, `MapToEntity()`, `Persist()` |
| CC-002 | Error | Nested if statements forbidden. Use guard clauses, pattern matching, or extraction. | `if (a) { if (b) { if (c) { ... } } }` | Early returns: `if (!a) return; if (!b) return; if (!c) return; ...` |
| CC-003 | Warning | Constructor parameters must be 4 or fewer. More suggests the class has too many responsibilities. | `ctor(IFoo, IBar, IBaz, IQux, IQuux, ICorge)` | Split into two classes with 3 dependencies each |
| CC-004 | Info | Single-implementation interfaces are a smell. Question whether the interface adds value. | `IFooService` with only `FooService` | Use the concrete class directly, or justify the interface (testability, DI boundary) |
| CC-005 | Warning | Prefer declarative style: LINQ over imperative loops, immutable records over mutable state, switch expressions over verbose conditionals. | `foreach` loop building a list with `if` checks | `.Where().Select().ToList()` |
| CC-006 | Warning | Class cohesion: all methods should use most fields. If methods cluster into groups using different field subsets, split the class. | Class with 8 fields where half the methods use fields 1-4 and the other half use fields 5-8 | Two classes, each with 4 fields and high cohesion |
| CC-007 | Info | No deep inheritance hierarchies or abstract base classes without clear justification. Prefer composition, delegates, or simple functions. | `AbstractBaseService<T> : BaseEntityService<T> : IService<T>` | A concrete class using composition or a `Func<>` parameter |

**Create**: PostToolUse hook (see [Hook Architecture](#hook-architecture) below)

**Add to CLAUDE.md**:
```
## Code Cleanliness Rules (Auto-Enforced)
- Methods MUST be under 20 lines of executable code; aim for under 15
- Nested if statements are forbidden -- use guard clauses or early returns
- Constructor parameters must be 4 or fewer
- Prefer declarative style: LINQ, switch expressions, pattern matching, immutable records
- See .claude/rules/code-cleanliness.md for the full rule set
```

**Create**: `.claude/skills/review-cleanliness/SKILL.md` that runs `git diff` and reviews all changed files against the rules file.

```yaml
---
name: review-cleanliness
description: Batch review of all staged changes against code cleanliness rules
allowed-tools: Read, Glob, Grep, Bash
context: fork
---
Review all staged files as a batch against .claude/rules/code-cleanliness.md.

Staged changes:
!`git diff --cached`

For each violation found, report [RULE-ID] [SEVERITY] description and file location.
```

---

## What to Change: HYBRID Agents

For each HYBRID agent, two things happen:
1. **Extract review rules** into a rules file and wire up a hook
2. **Slim down the agent** to remove the review checklist sections, keeping only planning/design content

### backend-architect

**Create**: `.claude/rules/backend.md`

```yaml
---
paths:
  - "**/*.cs"
  - "!**/*Test*.cs"
  - "!**/Tests/**"
---
```

| ID | Severity | Rule |
|----|----------|------|
| BE-001 | Error | No silent fallbacks: `??` with default values is forbidden on data lookups. Throw `NotFoundException` instead. |
| BE-002 | Error | No swallowed exceptions: `catch { return default; }` is forbidden. Log and rethrow. |
| BE-003 | Error | No `TrySomethingSafe()` methods that never fail. Failures must be explicit. |
| BE-004 | Warning | All public APIs must have XML doc comments. |
| BE-005 | Warning | Use async/await for all I/O operations. |
| BE-006 | Warning | Implement proper disposal patterns (`IDisposable`/`IAsyncDisposable`) for resources. |
| BE-007 | Info | Methods should be under 30 lines. |
| BE-008 | Warning | Dependencies must be explicit, minimal, and constructor-injected. |

Extract bad/good code examples from lines 41-68 and 117-124 of `backend-architect.md`.

**Hook filter**: `Edit`/`Write` on `**/*.cs` (exclude `**/*Tests.cs`, `**/*.Test.cs`, `**/Tests/**`)

**Slim the agent**: Remove the "Red Flag Patterns" section (lines 117-124) and "Self-Verification" checklist (lines 102-115). Keep the planning methodology, SOLID principles, GoF patterns, and "When Making Changes" sections.

**Add to CLAUDE.md**:
```
## Backend Rules (Auto-Enforced)
- NEVER use silent fallbacks (`??` with defaults, catch-return-default, TrySomething patterns)
- Failures must be explicit: throw exceptions, log and rethrow
- See .claude/rules/backend.md for the full rule set
```

### react-architect

**Create**: `.claude/rules/react.md`

```yaml
---
paths:
  - "**/*.tsx"
  - "**/*.ts"
  - "!**/*.test.ts"
  - "!**/*.test.tsx"
---
```

| ID | Severity | Rule |
|----|----------|------|
| RC-001 | Error | No prop drilling beyond 2 levels. Use composition or context. |
| RC-002 | Warning | Components over 200 lines likely need splitting. |
| RC-003 | Error | Never use `useEffect` for derived state. Use `useMemo` instead. |
| RC-004 | Error | No state synchronization between two sources. Redesign the architecture. |
| RC-005 | Warning | Business logic must not live in components. Extract to hooks or utility functions. |
| RC-006 | Warning | Data fetching in `useEffect` must have proper cleanup/cancellation. |
| RC-007 | Warning | No circular dependencies between feature modules. |
| RC-008 | Error | No `any` types in TypeScript. Use proper types or `unknown`. |

Extract from lines 99-107 of `react-architect.md`.

**Hook filter**: `Edit`/`Write` on `**/*.tsx`, `**/*.ts` in frontend source directories

**Slim the agent**: Remove the "Anti-Patterns to Flag" section (lines 99-107). Keep component hierarchy design, state management strategy, methodology, and output format sections.

### sentinel

**Create**: `.claude/rules/security-universal.md`

(No `paths:` frontmatter — security rules always load since security is universal across all file types.)

| ID | Severity | Rule |
|----|----------|------|
| SEC-001 | Critical | Direct entity lookups by ID on user-owned entities MUST include user scoping predicate. |
| SEC-002 | Critical | Never bypass user scoping filters on user-owned entities. |
| SEC-003 | Critical | UserId must come from authenticated identity, NEVER from request body/query/route input. |
| SEC-004 | Critical | All ID-based read/update/delete operations must verify user/owner ownership. |
| SEC-005 | High | All list/search/export/report/download endpoints must enforce user scoping. |
| SEC-006 | High | All endpoints touching user data must be authenticated. No anonymous access to user data. |

Extract from "Scary Pattern" Rules (lines 103-110) of `sentinel.md`.

**Hook filter**: `Edit`/`Write` on `**/*.cs`, `**/*.tsx`, `**/*.ts` (full-stack, security is always relevant)

**Keep the agent intact** for its full 8-step audit methodology. The hook handles quick pattern catches; the agent handles deep audits. Do NOT remove sections from the agent.

### idesign-architect

**Create**: `.claude/rules/idesign.md`

```yaml
---
paths:
  - "**/*Manager.cs"
  - "**/*Engine.cs"
  - "**/*Accessor.cs"
  - "**/*Controller.cs"
---
```

| ID | Severity | Rule |
|----|----------|------|
| ID-001 | Critical | FORBIDDEN: Manager class injecting or calling another Manager. |
| ID-002 | Critical | FORBIDDEN: Engine class injecting or calling another Engine. |
| ID-003 | Critical | FORBIDDEN: Accessor class injecting or calling another Accessor. |
| ID-004 | Error | Dependencies must flow downward only: Controller -> Manager -> Engine -> Accessor. |
| ID-005 | Warning | Design smell: Forks -- a Manager branching into two unrelated workflows should be split. |
| ID-006 | Warning | Design smell: Staircases -- call chains like Manager->Engine->Accessor->Engine->Accessor should be flattened. |
| ID-007 | Warning | Design smell: Functional decomposition -- breaking a Manager into sub-Managers violates ID-001. |

Include infrastructure whitelist (ILogger, IConfiguration, IOptions, SDK clients are NOT violations).

**Hook filter**: `Edit`/`Write` on `**/*Manager.cs`, `**/*Engine.cs`, `**/*Accessor.cs`

**Slim the agent**: Remove the "Violation Detection Methodology" section (lines 68-90) which is now the hook's job. Keep the layer definitions, planning guidance, feedback format, and communication style.

### sql-data-architect

**Create**: `.claude/rules/sql.md`

```yaml
---
paths:
  - "**/*Accessor.cs"
  - "**/Migrations/**"
  - "**/*DbContext.cs"
  - "**/*.sql"
---
```

| ID | Severity | Rule |
|----|----------|------|
| SQL-001 | Error | NEVER use GUIDs as clustered primary keys. Use `int IDENTITY` or `bigint IDENTITY`. |
| SQL-002 | Error | NEVER call `Database.Migrate()` on application startup. |
| SQL-003 | Warning | All schema changes must follow expand-contract pattern for zero-downtime. |
| SQL-004 | Error | Never rename or drop columns directly in production. |
| SQL-005 | Warning | Use `AsNoTracking()` for all read-only queries. |
| SQL-006 | Warning | Use keyset pagination. NEVER use `OFFSET/FETCH` at high page numbers. |
| SQL-007 | Info | Use `.Select()` projections for reads, not full entity loads. |

**Hook filter**: `Edit`/`Write` on `**/*Accessor.cs`, `**/Migrations/**`, `**/*DbContext.cs`, `**/*.sql`

**Slim the agent**: Remove anti-patterns table. Keep schema design philosophy, migration strategy, indexing guidance, and review methodology.

### table-storage-architect

**Create**: `.claude/rules/table-storage.md`

```yaml
---
paths:
  - "**/TableStorage/**"
  - "**/*TableEntity.cs"
---
```

| ID | Severity | Rule |
|----|----------|------|
| TS-001 | Critical | NEVER use a single constant PartitionKey for all entities (creates a hot partition). |
| TS-002 | Error | NEVER use GUIDs as PartitionKeys (destroys locality). |
| TS-003 | Error | Table scans are FORBIDDEN in production code. All queries must specify PartitionKey. |
| TS-004 | Error | Always handle continuation tokens. Never assume a query returns all results. |
| TS-005 | Warning | Always use ETag for conditional updates (optimistic concurrency). |
| TS-006 | Warning | Use query projection -- select only the properties you need. |

**Hook filter**: `Edit`/`Write` on Table Storage entity and accessor classes (project-specific path pattern)

**Slim the agent**: Remove anti-patterns table. Keep partition key design philosophy, mental model shift table, and design principles.

### modern-ui-agent

**Create**: `.claude/rules/ui-design.md`

```yaml
---
paths:
  - "**/*.tsx"
  - "**/*.css"
  - "**/*.scss"
---
```

| ID | Severity | Rule |
|----|----------|------|
| UI-001 | Warning | No saturated primary sidebars (solid blue/purple navs). Use neutral backgrounds. |
| UI-002 | Warning | No rainbow inconsistency. Establish a strict 2-3 color system beyond neutrals. |
| UI-003 | Info | Replace heavy borders with subtle shadows or background differentiation. |
| UI-004 | Warning | Create clear typographic hierarchy -- at least 3-4 distinct levels. No uniform text sizing. |
| UI-005 | Warning | Reserve saturated color for ONE primary action per view. Not every button is primary. |
| UI-006 | Error | Every interactive element MUST have hover, active, and focus states. |
| UI-007 | Warning | No sparse/empty pages. Use intentional whitespace with appropriate content density. |
| UI-008 | Warning | Status communication must use consistent color coding (success/warning/danger/info). |

Extract from anti-patterns (lines 25-35) and review checklist (lines 315-326) of `modern-ui-agent.md`.

**Hook filter**: `Edit`/`Write` on `**/*.tsx`, `**/*.css`, `**/*.scss` in frontend directories

**Slim the agent**: Remove the "Review Checklist" section (lines 315-326) which is now the hook's job. Keep the design system foundation, component patterns, and application process.

---

## What to Change: KEEP Agents

No changes needed for these agents:

- **azure-architect** -- Multi-step workflows, Azure CLI, Bicep generation, self-maintenance
- **data-clarifier** -- Entirely generative, on-demand, contextual judgment
- **debug-investigator** -- On-demand 7-phase workflow, requires Bash/WebSearch

---

## Hook Architecture

**Rules auto-load via `.claude/rules/`**

Since rules files now have `paths:` frontmatter, they auto-load when Claude touches relevant files. This eliminates the need for bash scripts that check file extensions and tell Claude to read rules files. The rules are already in context when they are needed.

### Hook Configuration in `.claude/settings.json`

Hook configuration lives in `.claude/settings.json` (not a separate `hooks.json`). Each entry has a `matcher` string (regex against tool name) and a `hooks` array. Each hook must specify `"type"` (`command`, `http`, `prompt`, or `agent`).

```json
{
  "hooks": {
    "PostToolUse": [
      {
        "matcher": "Edit|Write",
        "hooks": [
          {
            "type": "prompt",
            "prompt": "Check if the edit just made violates any rules loaded in context. If violations found, list each with [RULE-ID] [SEVERITY] description."
          }
        ]
      }
    ]
  }
}
```

Key differences from old format:
- Config lives in `.claude/settings.json`, not a separate `hooks.json`
- Each entry has a `matcher` string (regex against tool name) and a `hooks` array
- Each hook must specify `"type"` (`command`, `http`, `prompt`, `agent`)
- The matcher handles tool filtering; no bash script needed

### Handler Types

There are 4 handler types:
- `command`: Run a shell command
- `prompt`: Single-turn LLM evaluation. Perfect for per-edit rule checking. Returns `{ok: true/false, reason: "..."}`
- `agent`: Multi-turn verification with tool access. Can read files, run commands. Perfect for cross-file analysis.
- `http`: POST to an endpoint

Since rules auto-load via `.claude/rules/`, a `prompt` hook does not need to specify which rules to check — they are already in context. This eliminates all bash scripting for rule checking.

For cross-file analysis, use an `agent` hook:

```json
{
  "type": "agent",
  "prompt": "Trace the data flow from the edited file through managers and accessors. Verify user scoping reaches the data access layer."
}
```

The `prompt` type is the right choice for most per-edit rule checks. The `agent` type is reserved for deeper analysis that requires reading additional files or running commands.

---

## Migration Checklist

### Phase 1: Create rules files (no agent changes yet)
- [ ] Create `.claude/rules/` directory
- [ ] Create `code-cleanliness.md` with CC-001 through CC-007 and `paths:` frontmatter
- [ ] Create `backend.md` with BE-001 through BE-008 and `paths:` frontmatter
- [ ] Create `react.md` with RC-001 through RC-008 and `paths:` frontmatter
- [ ] Create `security-universal.md` with SEC-001 through SEC-006 (no `paths:` — always loaded)
- [ ] Create `idesign.md` with ID-001 through ID-007 and `paths:` frontmatter
- [ ] Create `sql.md` with SQL-001 through SQL-007 and `paths:` frontmatter
- [ ] Create `table-storage.md` with TS-001 through TS-006 and `paths:` frontmatter
- [ ] Create `ui-design.md` with UI-001 through UI-008 and `paths:` frontmatter

### Phase 2: Create hooks, skills, and CLAUDE.md template
- [ ] Remove `.claude/hooks/review-backend.sh` etc. (no longer needed — `prompt` hooks replace bash scripts)
- [ ] Remove `.claude/hooks/precommit-review.sh` — replace with `.husky/pre-commit` (Husky setup)
- [ ] Remove `hooks.json` reference — replace with `.claude/settings.json` using `prompt`/`agent` hooks
- [ ] Set up Husky: `npm install --save-dev husky && npx husky init`
- [ ] Create `.claude/settings.json` with `PostToolUse` `prompt` hook configuration
- [ ] Create `.claude/skills/` directory structure
- [ ] Create `.claude/skills/review-cleanliness/SKILL.md` with `context: fork` frontmatter
- [ ] Create `docs/design-system.md` (extracted from modern-ui-agent)
- [ ] Create `docs/architecture.md` (IDesign layers + project structure)
- [ ] Create lean CLAUDE.md template (~30-40 lines): 5 universal rules + key conventions

### Phase 3: Restructure agent memory
- [ ] Update memory instructions in all 5 memory-enabled agents to distinguish Tier 1 (→ CLAUDE.md) vs. Tier 2 (→ agent memory) knowledge
- [ ] Rewrite "Persistent Agent Memory" sections to say: *"Store only agent-specific observations here (recurring hotspots, regression warnings). Project-wide conventions and patterns belong in CLAUDE.md."*
- [ ] Add domain-specific guidance for what stays in memory vs. what gets promoted to CLAUDE.md
- [ ] Update `agent-architecture.md` Step 5 and Principle 6 to describe two-tier memory model
- [ ] Update `bootstrap-agents.md` Phase 5 to include memory tier guidance

### Phase 4: Update agents
- [ ] Delete `.claude/agents/code-cleanliness.md` (fully replaced by rules+hooks)
- [ ] Slim `.claude/agents/backend-architect.md` -- remove review checklists, keep planning content
- [ ] Slim `.claude/agents/react-architect.md` -- remove anti-patterns list, keep design content
- [ ] Slim `.claude/agents/idesign-architect.md` -- remove violation detection methodology, keep planning
- [ ] Slim `.claude/agents/sql-data-architect.md` -- remove anti-patterns table, keep design content
- [ ] Slim `.claude/agents/table-storage-architect.md` -- remove anti-patterns table, keep design content
- [ ] Slim `.claude/agents/modern-ui-agent.md` -- remove review checklist, keep design system content
- [ ] Keep `.claude/agents/sentinel.md` intact (hook supplements, doesn't replace)

### Phase 5: Update methodology docs
- [ ] Update `agent-architecture.md` to describe three-layer review architecture and two-tier memory model
- [ ] Update `bootstrap-agents.md` to include hook setup steps and memory tier guidance
- [ ] Document when to use the agent vs. relying on the hook

### Phase 6: Verification
- [ ] Test each hook by editing a file and confirming self-review triggers
- [ ] Verify CLAUDE.md rules are followed proactively during code generation
- [ ] Run a side-by-side comparison: old agent review vs. hook+rules review on the same change
- [ ] Confirm HYBRID agents still work for their planning role with review sections removed
- [ ] Verify memory-enabled agents store only Tier 2 knowledge in agent memory

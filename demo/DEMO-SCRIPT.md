# TaskBoard Demo Script

Step-by-step walkthrough demonstrating all 10 agents, 9 rule sets, 5 skills, pre-commit hook, agent memory, and security baseline tracking.

**Estimated time:** ~55 minutes total (or pick individual parts)

---

## Part 0: Bootstrap (5 min)

**Goal:** Show `/bootstrap-agents` detecting the stack and creating the `.claude/` ecosystem.

1. Open the `demo/` directory in a fresh terminal
2. Run `/bootstrap-agents`
3. Observe the output:
   - Stack detection: C# .NET 8, React/TypeScript, Azure Bicep, SQL Server, Azure Table Storage
   - Rules created in `.claude/rules/` for each detected technology
   - Agent definitions created in `.claude/agents/`
   - Skills created in `.claude/skills/`
   - Memory directories initialized
   - `CLAUDE.md` generated with project-specific guidance

**Key talking point:** One command sets up the entire quality architecture — rules, agents, skills, memory, and pre-commit hook — tailored to the detected stack.

---

## Part 1: Rules Layer (10 min)

**Goal:** Show rules auto-loading by file type, with Claude automatically flagging violations.

### 1a. Backend rules in action

1. Open `src/Api/TaskBoard.Api/Managers/DashboardManager.cs`
2. Ask Claude: "Review this file"
3. Observe rules triggered:
   - `idesign.md` → flags Manager → Manager dependency (ID-001)
   - `universal.md` → flags silent fallback (UNI-001), long method (UNI-002), nested ifs (UNI-003)
   - `backend.md` → flags default return on failure (BE-001)
   - `code-cleanliness.md` → flags imperative loop over declarative style (CC-002)
   - `CLAUDE.md` → universal rules (method length, nested ifs, constructor params)
   - `security-universal.md` → loaded but no violations (user scoping present via `userId`)

### 1b. Frontend rules in action

1. Open `src/Web/src/components/Layout/Sidebar.tsx`
2. Ask Claude: "Review this component"
3. Observe design system rules flagging:
   - Saturated dark background violating design-system.md
   - Missing hover states

### 1c. SQL rules in action

1. Open `sql/001-create-tables.sql`
2. Ask Claude: "Review this schema"
3. Observe: `sql.md` → flags GUID clustered primary key (SQL-001)

**Key talking point:** Rules auto-load based on file type. No manual configuration needed. Claude applies the right rules to the right files.

---

## Part 2: Pre-Commit Hook (10 min)

**Goal:** Show the hook blocking CRITICAL violations and allowing commits after fixes.

### 2a. Blocked commit

1. Stage violation files:
   ```bash
   git add src/Api/TaskBoard.Api/Accessors/TaskAccessor.cs
   git add src/Api/TaskBoard.Api/Infrastructure/AuthMiddleware.cs
   ```
2. Attempt commit: `git commit -m "Add task accessor and auth"`
3. Hook blocks with CRITICAL findings:
   - SEC-001: BOLA/IDOR in TaskAccessor (no user scoping)
   - SEC-006: Hardcoded secret in AuthMiddleware

### 2b. Fix and commit

1. Fix the CRITICAL violations (add user scoping, remove hardcoded key)
2. Re-stage and commit — hook passes

**Key talking point:** The pre-commit hook acts as a safety net, catching critical security and architecture violations before they reach the repo.

---

## Part 3: Agent Deep Dives (20 min)

**Goal:** Invoke each agent on its target files. Each finds unique issues the others miss.

### 3a. idesign-architect

```
Review DashboardManager.cs, TaskController.cs, and PriorityEngine.cs for IDesign compliance
```

Findings: ID-001 (Manager → Manager), ID-002 (Engine → Engine), ID-004 (two Managers in Controller)

### 3b. backend-architect

```
Review DashboardManager.cs, TaskEntity.cs, and ServiceRegistration.cs
```

Findings: UNI-001/002/003/004, BE-001/004

### 3c. sentinel

```
Run /sentinel-review on TaskAccessor.cs, useTasks.ts, main.bicep, AuthMiddleware.cs
```

Findings: SEC-001 (BOLA), SEC-002 (userId from URL), SEC-006 (hardcoded secrets)

### 3d. sql-data-architect

```
Review 001-create-tables.sql, TaskAccessor.cs, and TaskDbContext.cs
```

Findings: SQL-001 (GUID PK), SQL-002 (Migrate at startup), SQL-005/006/007

### 3e. table-storage-architect

```
Review AuditLogAccessor.cs
```

Findings: TS-001 (constant PartitionKey), TS-002 (table scan), TS-003 (no continuation tokens)

### 3f. azure-deployment-architect

```
Review main.bicep and deploy.yml for Azure deployment best practices
```

Findings: Missing Key Vault, managed identity, staging slot, health checks

### 3g. react-architect

```
Review DashboardPage.tsx, TaskFilters.tsx, TaskDetailModal.tsx, and useTasks.ts
```

Findings: RC-001 (prop drilling), RC-002/005 (god component), RC-003/004 (useEffect derived state)

### 3h. modern-ui-agent

```
Review Sidebar.tsx and globals.css against the design system
```

Findings: UI-001 (saturated sidebar), UI-004 (no hover states), UI-005/006 (spacing/borders)

### 3i. data-clarifier

```
Audit DashboardPage.tsx for cognitive load and data presentation
```

Findings: 15+ metrics at equal visual weight, no progressive disclosure, wall of numbers

### 3j. debug-investigator

```
The dashboard sometimes returns empty data even when tasks exist. Investigate.
```

Root cause: DashboardManager silently catches exceptions from TaskManager and returns an empty DTO. The real error is hidden by the fallback.

**Key talking point:** Each agent brings a unique perspective. The same file (DashboardManager.cs) gets different — and complementary — findings from idesign-architect, backend-architect, and debug-investigator.

---

## Part 4: Skills Walkthrough (10 min)

**Goal:** Run each of the 5 built-in skills.

### 4a. `/review-cleanliness`

Stage multiple files and run the skill. Shows batch review catching UNI-* and CC-* violations across all staged files.

### 4b. `/sentinel-review`

Run on changed files. Shows security-focused review finding SEC-* violations with severity ratings.

### 4c. `/debug-investigate`

Point at the dashboard's empty-data bug. Shows scientific debugging methodology: symptoms → hypotheses → evidence → root cause.

### 4d. `/clarify-data`

Point at DashboardPage.tsx. Shows cognitive load audit with specific recommendations for progressive disclosure and visual hierarchy.

### 4e. `/bootstrap-agents`

Already demoed in Part 0. Reference the output to show the full `.claude/` ecosystem.

---

## Part 5: Agent Memory (5 min)

**Goal:** Show findings persisting across sessions.

1. After running sentinel in Part 3c, check `.claude/agent-memory/sentinel/`
2. Show the security findings saved with timestamps
3. Start a new Claude conversation
4. Ask: "What security issues were found in the last review?"
5. Claude recalls findings from memory without re-scanning

**Key talking point:** Agent memory means quality context survives across conversations. Teams build up a knowledge base over time.

---

## Part 6: Security Baseline (5 min)

**Goal:** Show the full baseline lifecycle.

### 6a. Empty baseline

1. Show `.claude/agent-memory/sentinel/security-baseline.json` is empty or doesn't exist
2. This is the starting state for any new project

### 6b. Populated baseline

1. Run `/sentinel-review` — baseline gets populated with all SEC-* findings
2. Show the baseline file with finding hashes, severities, and file locations

### 6c. Fix and resolve

1. Fix SEC-001 in TaskAccessor.cs (add user scoping)
2. Run `/sentinel-review` again — finding is marked as resolved in baseline

### 6d. Regression detection

1. Reintroduce the SEC-001 violation
2. Commit attempt — pre-commit hook detects the regression (finding was previously resolved)
3. Hook blocks with "Security regression detected"

**Key talking point:** The baseline creates accountability. Once a vulnerability is fixed, it can never silently return.

---

## Appendix: Correct Files for Comparison

These files pass all agent reviews cleanly:

- `TaskManager.cs` — correct IDesign Manager pattern
- `StatCard.tsx` — clean, focused React component
- `TaskRow.tsx` — clean row component
- `Header.tsx` — design system compliant
- `taskApi.ts` — clean API service

Use these as before/after references when demonstrating fixes.

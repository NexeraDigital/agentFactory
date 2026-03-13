# Agent Factory Test Plan

**Target project:** `C:\GitHub\temp\test7` (TaskBoard)
**Source branch:** `russ/suggestions`

---

## Prerequisites

- [ ] test7 repo exists at `C:\GitHub\temp\test7` with initial commit
- [ ] agentFactory `russ/suggestions` branch is pushed to GitHub
- [ ] Claude Code CLI available in terminal

---

## Test Group 1: Bootstrap (`/bootstrap-agents`)

Run all tests from within `C:\GitHub\temp\test7`.

| # | Test | Steps | Expected Result | Pass |
|---|------|-------|----------------|------|
| 1.1 | Stack detection | Run `/bootstrap-agents` in test7 | Detects: .NET 9 backend, React/TypeScript frontend, SQL (EF Core), Azure Table Storage, Bicep infra | [ ] |
| 1.2 | Profile selection | Observe recommended profile | Recommends "Full-Stack" profile with all 10 agents | [ ] |
| 1.3 | Rules created | Check `.claude/rules/` | Contains all 9 rule files with `paths:` adapted for `.cs`, `.tsx`, `.sql`, `.bicep` | [ ] |
| 1.4 | Agents created | Check `.claude/agents/` | Contains all 10 agent `.md` files with `<!-- ADAPT -->` sections filled | [ ] |
| 1.5 | Skills created | Check `.claude/skills/` | Has `debug-investigate`, `clarify-data`, `review-cleanliness`, `security-review` | [ ] |
| 1.6 | Pre-commit hook | Check `.husky/pre-commit` or `.git/hooks/pre-commit` | Installed with correct `BACKEND_PATTERN` / `FRONTEND_PATTERN` | [ ] |
| 1.7 | Agent memory dirs | Check `.claude/agent-memory/` | Has dirs for sentinel, react-architect, sql-data-architect, table-storage-architect, data-clarifier | [ ] |
| 1.8 | CLAUDE.md created | Check root `CLAUDE.md` | Contains agent dispatch table and universal rules | [ ] |
| 1.9 | Docs created | Check `docs/` | `architecture.md` and `design-system.md` populated with project context | [ ] |
| 1.10 | Security baseline | Check `.claude/security-baseline.md` | Created with empty Active/Exempted/Resolved sections | [ ] |

---

## Test Group 2: Rule Enforcement (Clean Code)

Ask Claude to review each file. Expect no violations on clean files.

| # | Test | Steps | Expected Result | Pass |
|---|------|-------|----------------|------|
| 2.1 | TasksController.cs | Ask Claude to review `src/TaskBoard.Api/Controllers/TasksController.cs` | No CRITICAL/HIGH violations. backend-architect, sentinel approve | [ ] |
| 2.2 | TaskManager.cs | Ask Claude to review `src/TaskBoard.Api/Managers/TaskManager.cs` | idesign-architect confirms: no Manager->Manager deps, proper layer usage | [ ] |
| 2.3 | PriorityEngine.cs | Ask Claude to review `src/TaskBoard.Api/Engines/PriorityEngine.cs` | No Engine->Engine deps, pure logic confirmed | [ ] |
| 2.4 | TaskAccessor.cs | Ask Claude to review `src/TaskBoard.Api/Accessors/TaskAccessor.cs` | sql-data-architect confirms: AsNoTracking, projections, keyset pagination, user scoping | [ ] |
| 2.5 | ActivityLogAccessor.cs | Ask Claude to review `src/TaskBoard.Api/Accessors/ActivityLogAccessor.cs` | table-storage-architect confirms: proper PartitionKey, continuation tokens, denormalization | [ ] |
| 2.6 | Dashboard.tsx | Ask Claude to review `src/TaskBoard.Web/src/components/Dashboard.tsx` | react-architect confirms: <200 lines, useMemo for derived, no prop drilling, strict TS | [ ] |
| 2.7 | main.bicep | Ask Claude to review `infra/main.bicep` | azure-deployment-architect confirms: parameterized, Key Vault, managed identity | [ ] |

---

## Test Group 3: Violation Detection

Ask Claude to review each violation file. Expect specific rule IDs to be flagged.

| # | File | Steps | Expected Flags | Pass |
|---|------|-------|---------------|------|
| 3.1 | ViolationController.cs | Ask Claude to review `violations/ViolationController.cs` | SEC-001 (BOLA), SEC-002 (userId from body), SEC-004 (no auth), SEC-005 (SQL injection) | [ ] |
| 3.2 | ViolationManager.cs | Ask Claude to review `violations/ViolationManager.cs` | ID-001 (Manager->Manager), ID-008 (Fork), UNI-004 (5 params) | [ ] |
| 3.3 | ViolationEngine.cs | Ask Claude to review `violations/ViolationEngine.cs` | ID-002 (Engine->Engine), UNI-001 (silent fallback x2), UNI-002 (>20 lines), UNI-003 (nested ifs) | [ ] |
| 3.4 | ViolationAccessor.cs | Ask Claude to review `violations/ViolationAccessor.cs` | SQL-002 (Migrate), SQL-004 (N+1), SQL-005 (AsNoTracking), SQL-006 (OFFSET), SQL-007 (no projection), SEC-001 | [ ] |
| 3.5 | ViolationTableAccessor.cs | Ask Claude to review `violations/ViolationTableAccessor.cs` | TS-001 (constant PK), TS-002 (table scan), TS-003 (missing continuation), TS-005 (GUID PK) | [ ] |
| 3.6 | ViolationComponent.tsx | Ask Claude to review `violations/ViolationComponent.tsx` | RC-002/CC-003 (>200 lines), RC-003 (useEffect derived), RC-004 (state sync), RC-005 (business logic), RC-007 (any). UI: saturated sidebar, spacing, multiple primaries | [ ] |
| 3.7 | violation.bicep | Ask Claude to review `violations/violation.bicep` | Hardcoded secrets, no @secure(), public access, no managed identity | [ ] |

---

## Test Group 4: Skills

| # | Skill | Steps | Expected Result | Pass |
|---|-------|-------|----------------|------|
| 4.1 | `/review-cleanliness` | Stage violation files, run `/review-cleanliness` | Reports issues with rule IDs, severities, and file:line locations | [ ] |
| 4.2 | `/security-review` | Stage `ViolationController.cs`, run `/security-review` | sentinel produces findings with SEC-xxx IDs, evidence, remediation | [ ] |
| 4.3 | `/debug-investigate` | Introduce a deliberate bug, run `/debug-investigate` | debug-investigator follows 7-phase methodology, identifies root cause | [ ] |
| 4.4 | `/clarify-data` | Point at `Dashboard.tsx`, run `/clarify-data` | data-clarifier audits cognitive load, proposes Level 1/2/3 hierarchy | [ ] |

---

## Test Group 5: Pre-Commit Hook

| # | Test | Steps | Expected Result | Pass |
|---|------|-------|----------------|------|
| 5.1 | Clean commit | Stage only clean files (e.g., `TasksController.cs`), run `git commit` | Hook runs, outputs PASS for backend and frontend domains | [ ] |
| 5.2 | Violation commit (backend) | Stage `ViolationController.cs`, run `git commit` | Hook blocks commit with CRITICAL: SEC violations | [ ] |
| 5.3 | Violation commit (frontend) | Stage `ViolationComponent.tsx`, run `git commit` | Hook reports warnings or CRITICAL for React violations | [ ] |
| 5.4 | Large changeset | Stage >50 files, run `git commit` | Hook skips AI review (threshold message) | [ ] |
| 5.5 | Regression check | Run `/security-review`, then stage a file re-introducing a resolved finding | Sentinel regression check detects re-introduction of known finding | [ ] |

---

## Test Group 6: Agent Collaboration (Planning Sessions)

| # | Scenario | Steps | Expected Agents | Pass |
|---|----------|-------|----------------|------|
| 6.1 | Full-stack feature | Ask Claude to plan "add user settings feature" | backend-architect, react-architect, idesign-architect, sentinel all participate | [ ] |
| 6.2 | Data feature | Ask Claude to plan "add invoice table" | sql-data-architect, idesign-architect, backend-architect participate | [ ] |
| 6.3 | Frontend + data | Ask Claude to plan "add activity dashboard" | react-architect, data-clarifier, modern-ui-agent, table-storage-architect participate | [ ] |
| 6.4 | Infrastructure | Ask Claude to plan "deploy to production" | azure-deployment-architect leads, sentinel reviews security posture | [ ] |

---

## Test Group 7: Agent Memory Persistence

| # | Test | Steps | Expected Result | Pass |
|---|------|-------|----------------|------|
| 7.1 | Sentinel memory | Run sentinel audit, check `.claude/agent-memory/sentinel/MEMORY.md` | Memory updated with project-specific observations | [ ] |
| 7.2 | Security baseline | Run `/security-review`, check `.claude/security-baseline.md` | Findings persisted with SENT-xxx IDs | [ ] |
| 7.3 | Finding resolution | Fix a finding, re-run `/security-review` | Finding moves to Resolved section | [ ] |

---

## Coverage Matrix

### Agents Exercised

| Agent | Test Groups |
|-------|------------|
| backend-architect | 2.1, 2.2, 3.2, 3.3, 6.1, 6.2 |
| react-architect | 2.6, 3.6, 6.1, 6.3 |
| sentinel | 2.1, 3.1, 4.2, 5.2, 5.5, 6.1, 6.4, 7.1, 7.2, 7.3 |
| idesign-architect | 2.2, 2.3, 3.2, 6.1, 6.2 |
| sql-data-architect | 2.4, 3.4, 6.2 |
| table-storage-architect | 2.5, 3.5, 6.3 |
| modern-ui-agent | 3.6, 6.3 |
| azure-deployment-architect | 2.7, 3.7, 6.4 |
| data-clarifier | 4.4, 6.3 |
| debug-investigator | 4.3 |

### Rule Sets Exercised

| Rule Set | Test Groups |
|----------|------------|
| UNI (Universal) | 3.2, 3.3 |
| SEC (Security) | 3.1, 3.4, 4.2, 5.2 |
| BE (Backend) | 2.1, 2.2 |
| ID (IDesign) | 2.2, 2.3, 3.2 |
| SQL | 2.4, 3.4 |
| TS (Table Storage) | 2.5, 3.5 |
| RC (React) | 2.6, 3.6 |
| UI (Design) | 3.6 |
| CC (Code Cleanliness) | 3.6 |

### Skills Exercised

| Skill | Test |
|-------|------|
| `/bootstrap-agents` | Group 1 |
| `/review-cleanliness` | 4.1 |
| `/security-review` | 4.2, 5.5, 7.2, 7.3 |
| `/debug-investigate` | 4.3 |
| `/clarify-data` | 4.4 |

**Total: 10 agents, 9 rule sets, 5 skills covered across 37 test cases.**

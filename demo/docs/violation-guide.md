# Violation Guide

Complete mapping of every intentional violation in the TaskBoard demo to the rule it breaks and the agent that detects it.

## Backend Violations

| File | Violation | Rule | Detecting Agent |
|------|-----------|------|-----------------|
| `DashboardManager.cs` | Injects `ITaskManager` (Manager → Manager) | ID-001 | idesign-architect |
| `DashboardManager.cs` | Silent fallback — catches exception, returns empty DTO | UNI-001, BE-001 | backend-architect |
| `DashboardManager.cs` | `GetDashboardAsync` exceeds 20 executable lines | UNI-002 | backend-architect |
| `DashboardManager.cs` | Deeply nested if/else blocks | UNI-003 | backend-architect |
| `TaskController.cs` | Injects two Managers (`ITaskManager` + `IDashboardManager`) | ID-004 | idesign-architect |
| `PriorityEngine.cs` | Injects `IDeadlineEngine` (Engine → Engine dependency) | ID-002 | idesign-architect |
| `TaskAccessor.cs` | `GetTaskByIdAsync` has no user scoping (BOLA/IDOR) | SEC-001 | sentinel |
| `TaskAccessor.cs` | Update/Delete operations lack user scoping | SEC-001, SEC-003 | sentinel |
| `TaskAccessor.cs` | Read queries missing `AsNoTracking()` | SQL-005 | sql-data-architect |
| `TaskAccessor.cs` | Uses OFFSET/FETCH pagination | SQL-006 | sql-data-architect |
| `TaskAccessor.cs` | Loads full entities instead of `.Select()` projections | SQL-007 | sql-data-architect |
| `TaskAccessor.cs` | Leaks EF entity types across layer boundaries | ID-007 | idesign-architect |
| `AuditLogAccessor.cs` | Constant PartitionKey `"AuditLog"` for all entries | TS-001 | table-storage-architect |
| `AuditLogAccessor.cs` | Table scan when `userId` is null (no PartitionKey filter) | TS-002 | table-storage-architect |
| `AuditLogAccessor.cs` | No continuation token handling (max 1,000 entities) | TS-003 | table-storage-architect |
| `TaskEntity.cs` | Blocking async with `.Result` (deadlock risk) | BE-004 | backend-architect |
| `TaskDbContext.cs` | `Database.Migrate()` at startup (race condition) | SQL-002 | sql-data-architect |
| `ServiceRegistration.cs` | Constructor with 6 parameters | UNI-004 | backend-architect |
| `AuthMiddleware.cs` | Hardcoded API key in source code and comment | SEC-006 | sentinel |
| `AuthMiddleware.cs` | Internal details leaked in error response | SEC-006 | sentinel |

## Frontend Violations

| File | Violation | Rule | Detecting Agent |
|------|-----------|------|-----------------|
| `DashboardPage.tsx` | Component far exceeds 200 lines | CC-003 | react-architect |
| `DashboardPage.tsx` | Business logic mixed with presentation | RC-002 | react-architect |
| `DashboardPage.tsx` | No separation of concerns (fetch + compute + render) | RC-005 | react-architect |
| `DashboardPage.tsx` | 15+ metrics with no visual hierarchy | (cognitive load) | data-clarifier |
| `TaskFilters.tsx` | Derived state via useEffect (should be computed inline) | RC-003 | react-architect |
| `TaskFilters.tsx` | Unnecessary useEffect for state synchronization | RC-004 | react-architect |
| `TaskDetailModal.tsx` | Excessive prop drilling (userName, userRole, etc.) | RC-001 | react-architect |
| `useTasks.ts` | `userId` read from URL search params (user-controlled) | SEC-002 | sentinel |
| `Sidebar.tsx` | Saturated dark blue background (#1a237e) | UI-001 | modern-ui-agent |
| `Sidebar.tsx` | No hover states on navigation items | UI-004 | modern-ui-agent |
| `globals.css` | Spacing not on 8px grid (5px, 7px, 10px, 15px) | UI-005 | modern-ui-agent |
| `globals.css` | Heavy borders everywhere instead of shadows | UI-006 | modern-ui-agent |

## Infrastructure Violations

| File | Violation | Rule | Detecting Agent |
|------|-----------|------|-----------------|
| `main.bicep` | Hardcoded SQL admin password in variable | SEC-006 | sentinel, azure-deployment-architect |
| `main.bicep` | Missing Azure Key Vault module | (best practice) | azure-deployment-architect |
| `main.bicep` | Missing Managed Identity | (best practice) | azure-deployment-architect |
| `app-service.bicep` | No staging deployment slot | (best practice) | azure-deployment-architect |
| `app-service.bicep` | No health check path configured | (best practice) | azure-deployment-architect |
| `deploy.yml` | Deploys directly to production (no staging slot swap) | (best practice) | azure-deployment-architect |
| `deploy.yml` | No health check after deployment | (best practice) | azure-deployment-architect |
| `deploy.yml` | No rollback strategy | (best practice) | azure-deployment-architect |
| `deploy.yml` | Missing environment protection rules | (best practice) | azure-deployment-architect |

## SQL Violations

| File | Violation | Rule | Detecting Agent |
|------|-----------|------|-----------------|
| `001-create-tables.sql` | GUID (`UNIQUEIDENTIFIER`) as clustered primary key | SQL-001 | sql-data-architect |
| `001-create-tables.sql` | Both Tasks and AuditLog tables use GUID clustered PKs | SQL-001 | sql-data-architect |

## Correct Files (Should Pass Review)

| File | Why It's Correct |
|------|-----------------|
| `TaskManager.cs` | Clean IDesign Manager: single use case, downward dependencies only |
| `StatCard.tsx` | Focused component, props-driven, no side effects |
| `TaskRow.tsx` | Clean row component with single responsibility |
| `TaskListPage.tsx` | Delegates fetching to hook, rendering to children |
| `Header.tsx` | Follows design system: neutral background, proper typography |
| `taskApi.ts` | Clean API service with explicit error handling |

## Agent Coverage Summary

| Agent | # Violations Found | Primary Files |
|-------|-------------------|---------------|
| idesign-architect | 4 | DashboardManager, TaskController, PriorityEngine, TaskAccessor |
| backend-architect | 6 | DashboardManager, TaskEntity, ServiceRegistration |
| sentinel | 6 | TaskAccessor, useTasks, main.bicep, AuthMiddleware |
| sql-data-architect | 5 | 001-create-tables.sql, TaskAccessor, TaskDbContext |
| table-storage-architect | 3 | AuditLogAccessor |
| azure-deployment-architect | 6 | main.bicep, app-service.bicep, deploy.yml |
| react-architect | 6 | DashboardPage, TaskFilters, TaskDetailModal, useTasks |
| modern-ui-agent | 4 | Sidebar, globals.css |
| data-clarifier | 1 | DashboardPage (cognitive load audit) |
| debug-investigator | 1 | DashboardManager (silent fallback hides real bug) |

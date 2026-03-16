# TaskBoard Demo

A minimal full-stack Kanban task management app designed to showcase the complete agentFactory quality architecture: 10 agents, 9 rule sets, 5 skills, pre-commit hook, agent memory, and security baseline tracking.

## What This Demo Shows

TaskBoard contains a mix of **correct code** and **intentional violations** across every layer of the stack. This lets you see each agent, rule, and skill in action on realistic code patterns.

### Stack

| Layer | Technology |
|-------|-----------|
| Backend | C# .NET 8 Minimal API with IDesign architecture |
| Frontend | React 18 + TypeScript + Vite |
| Data | SQL Server (EF Core) + Azure Table Storage |
| Infrastructure | Azure Bicep + GitHub Actions CI/CD |

### Agent Coverage

Every one of the 10 agents has specific files that exercise its detection capabilities:

| Agent | Target Files |
|-------|-------------|
| idesign-architect | DashboardManager, TaskController, PriorityEngine |
| backend-architect | DashboardManager, TaskEntity, ServiceRegistration |
| sentinel | TaskAccessor, useTasks, main.bicep, AuthMiddleware |
| sql-data-architect | 001-create-tables.sql, TaskAccessor, TaskDbContext |
| table-storage-architect | AuditLogAccessor |
| azure-deployment-architect | main.bicep, deploy.yml |
| react-architect | DashboardPage, TaskFilters, TaskDetailModal |
| modern-ui-agent | Sidebar, globals.css |
| data-clarifier | DashboardPage (15+ unorganized metrics) |
| debug-investigator | DashboardManager (silent fallback hides bug) |

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 20+
- Azure CLI (for infrastructure deployment)

### Run the Demo

Follow the step-by-step walkthrough in [DEMO-SCRIPT.md](DEMO-SCRIPT.md).

For a complete mapping of every violation to its rule and detecting agent, see [docs/violation-guide.md](docs/violation-guide.md).

## Project Structure

```
demo/
├── src/
│   ├── Api/TaskBoard.Api/     # .NET 8 backend (IDesign layers)
│   ├── Web/                   # React frontend
│   └── Shared/                # Shared type definitions
├── infra/                     # Azure Bicep templates
├── sql/                       # Database scripts
├── docs/                      # Violation guide
├── .github/workflows/         # CI/CD pipeline
├── DEMO-SCRIPT.md             # Step-by-step walkthrough
└── README.md                  # This file
```

## Key Design Decisions

**DashboardManager.cs** is the "Rosetta Stone" — it concentrates violations from 3+ agents (idesign, backend, universal rules) in a single file for a fast opening demo.

**Correct files** (TaskManager.cs, StatCard.tsx, TaskRow.tsx, Header.tsx) demonstrate that agents also praise good patterns and serve as before/after references.

## Violation Summary

- **42 total intentional violations** across 19 files
- **9 rule sets** exercised (universal, backend, security, SQL, table storage, IDesign, code cleanliness, UI, React)
- **10 agents** with unique detection targets
- **5 skills** demonstrable on this codebase

See [docs/violation-guide.md](docs/violation-guide.md) for the complete matrix.

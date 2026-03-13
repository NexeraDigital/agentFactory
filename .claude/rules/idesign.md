---
description: "IDesign architecture rules applied to all files. Enforces layer definitions, forbidden dependencies, and service boundary rules for IDesign methodology."
---

# IDesign Architecture Rules

These rules enforce the IDesign layered service architecture methodology. They apply to all backend code to ensure correct layer separation and dependency direction.

| ID | Severity | Rule | Details |
|----|----------|------|---------|
| ID-001 | **Critical** | No Manager → Manager dependencies | Managers must NEVER inject or call other Managers. If cross-manager functionality is needed, extract shared logic into an Engine or have the Controller call multiple Managers separately. |
| ID-002 | **Critical** | No Engine → Engine dependencies | Engines must NEVER inject or call other Engines. Engines contain pure domain logic; if two engines need to collaborate, a Manager coordinates them. |
| ID-003 | **Critical** | No Accessor → Accessor dependencies | Accessors must NEVER inject or call other Accessors. Each accessor owns its own data source independently. |
| ID-004 | **High** | Dependencies flow top-down only | Controllers → Managers → Engines → Accessors. Never call upward. Managers may inject Engines, Accessors, ILogger. Engines may inject Accessors, ILogger. Accessors may inject IConfiguration, ILogger, SDK clients. |
| ID-005 | **High** | Correct layer placement | Managers (nouns) orchestrate use-case workflows. Engines (verbs) encapsulate reusable business rules. Accessors (business verbs) encapsulate data/resource access. Utilities handle cross-cutting concerns. |
| ID-006 | **Medium** | No design smells | Reject Forks (manager branching into unrelated workflows), Staircases (Manager→Engine→Accessor→Engine→Accessor chains), and Functional Decomposition (sub-managers). |
| ID-007 | **Medium** | Accessors are NOT simple CRUDs | Accessors express business-meaningful data operations, not generic CRUD wrappers. Method names should describe the business operation, not the database operation. |

## Infrastructure Whitelist (Not Violations)

These dependencies are ALLOWED and should NOT be flagged as layer violations:
- `ILogger<T>`, `IConfiguration`, `IOptions<T>` (infrastructure)
- Interfaces from Contracts (IOrderManager, IPricingEngine, etc.)
- SDK clients (TableClient, BlobClient, HttpClient, etc.)
- Utility classes (helpers, extensions)

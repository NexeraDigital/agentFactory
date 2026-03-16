---
description: "IDesign architecture rules. Enforces layer definitions, forbidden dependencies, and service boundary rules for IDesign methodology."
# ADAPT: add paths: to scope to your backend file patterns (e.g. ["**/*.cs"])
---

# IDesign Architecture Rules

These rules enforce the IDesign layered service architecture methodology. They apply to all backend code to ensure correct layer separation and dependency direction.

| ID | Severity | Rule | Details |
|----|----------|------|---------|
| ID-001 | **Critical** | No synchronous Manager → Manager dependencies | Managers must NEVER inject or synchronously call other Managers. If cross-manager functionality is needed, extract shared logic into an Engine. Queued/async messaging between Managers is permitted (but queue to at most one Manager; use pub/sub if recipients vary). |
| ID-002 | **Critical** | No Engine → Engine dependencies | Engines must NEVER inject or call other Engines. Engines contain pure domain logic; if two engines need to collaborate, a Manager coordinates them. |
| ID-003 | **Critical** | No Accessor → Accessor dependencies | Accessors must NEVER inject or call other Accessors. Each owns its own data source independently. |
| ID-004 | **Critical** | Single Manager per use case | Clients must call exactly ONE Manager per use case. Calling multiple Managers from a Client couples them through the Client layer. |
| ID-005 | **High** | Dependencies flow top-down only | Clients → Managers → Engines → Accessors. Never call upward. Managers may inject Engines, Accessors, ILogger. Engines may inject Accessors, ILogger. Accessors may inject IConfiguration, ILogger, SDK clients. |
| ID-006 | **High** | Correct layer placement | Clients (who) handle presentation/entry points. Managers (nouns) orchestrate use-case workflows and are near-expendable — if requirements change, only Managers should change. Engines (verbs) encapsulate reusable business rules and are rare — only create when truly reusable across Managers. Accessors (business verbs) encapsulate data/resource access. Utilities handle cross-cutting concerns. |
| ID-007 | **High** | Data contracts at layer boundaries | Pass only primitives, arrays of primitives, data contracts, or arrays of data contracts between layers. Never leak entities, `IQueryable`, or ORM types across boundaries. Each layer interprets data in its own context. |
| ID-008 | **Medium** | No design smells | Reject Forks (Manager branching into unrelated workflows), Staircases (Manager→Engine→Accessor→Engine→Accessor chains), and Functional Decomposition (sub-Managers). |
| ID-009 | **Medium** | Accessors are NOT simple CRUD | Accessors express business-meaningful data operations, not generic CRUD wrappers. Method names should describe the business operation, not the database operation. |
| ID-010 | **Medium** | Closed architecture by default | Components call only the layer immediately below. Managers may call Engines and Accessors (semi-open where justified). Engines and Accessors must not skip layers or call sideways. |
| ID-011 | **Medium** | Engines and Accessors are synchronous only | No queuing, publishing, or subscribing for Engines or Accessors. Only Managers participate in async messaging. |

## Infrastructure Whitelist (Not Violations)

These dependencies are ALLOWED and should NOT be flagged as layer violations:
- `ILogger<T>`, `IConfiguration`, `IOptions<T>` (infrastructure)
- Interfaces from Contracts (IOrderManager, IPricingEngine, etc.)
- SDK clients (TableClient, BlobClient, HttpClient, etc.)
- Utility classes (helpers, extensions)

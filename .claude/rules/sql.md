---
description: "SQL/relational database rules. Enforces schema design, query performance, and migration safety patterns."
# ADAPT: add paths: to scope to your data access file patterns (e.g. ["**/*.cs"])
---

# SQL Data Rules

| ID | Severity | Rule | Details |
|----|----------|------|---------|
| SQL-001 | **Critical** | No GUIDs as clustered primary keys | GUIDs cause severe index fragmentation and page splits. Use `int/bigint IDENTITY` as clustered PK. If GUIDs needed externally, add as non-clustered unique index. |
| SQL-002 | **Critical** | No `Database.Migrate()` at startup | Causes race conditions in multi-instance deployments and startup timeouts. Apply migrations via CI/CD pipeline only. |
| SQL-003 | **High** | Expand-contract for schema changes | All production schema changes must be backward-compatible with the previous app version. Add new → backfill → switch reads → drop old. Never rename or drop columns directly. |
| SQL-004 | **High** | No N+1 queries | Use `.Include()` / `.ThenInclude()` for eager loading. Use `.AsSplitQuery()` for multi-level includes to avoid Cartesian explosion. Or use Dapper for read paths. |
| SQL-005 | **High** | `AsNoTracking()` for read-only queries | Change tracker overhead is significant at scale. All read-only queries should use `AsNoTracking()`. |
| SQL-006 | **Medium** | Keyset pagination for large datasets | Never use `OFFSET/FETCH` at high page numbers — it degrades linearly. Use `WHERE Id > @lastId ORDER BY Id` (keyset/cursor pagination). |
| SQL-007 | **Medium** | `.Select()` projections to DTOs | Avoid `SELECT *` / loading full entities for read operations. Project to DTOs to reduce bandwidth and enable covering index usage. |

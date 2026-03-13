---
description: "Azure Table Storage / Cosmos DB Table API rules. Enforces partition key design, query performance, and NoSQL data modeling patterns."
globs: <!-- ADAPT: e.g. "**/*.cs" — set to your data access file patterns -->
---

# Table Storage Rules

| ID | Severity | Rule | Details |
|----|----------|------|---------|
| TS-001 | **Critical** | No single constant PartitionKey | Using one PartitionKey for all entities caps throughput at ~2,000 ops/sec and prevents load balancing. Use business-meaningful keys with sufficient cardinality. |
| TS-002 | **Critical** | No table scans in production | Queries without PartitionKey scan the entire table — extreme cost and latency. Every query must include PartitionKey. Redesign the data model if a table scan seems necessary. |
| TS-003 | **High** | Always handle continuation tokens | Queries return max 1,000 entities per response. Never assume all results are returned. Use `AsyncPageable<T>` or propagate continuation tokens to callers. |
| TS-004 | **High** | Denormalize by default | Table Storage is not relational. JOINs don't exist. Duplicate data to optimize reads. Use intra-partition secondary indexes (EGTs for consistency) or inter-partition indexes (queues for eventual consistency). |
| TS-005 | **Medium** | No GUIDs as PartitionKeys | GUIDs distribute well but prevent range queries and meaningful grouping. Use business-meaningful keys (TenantId, UserId, DepartmentName). |
| TS-006 | **Medium** | No append-only timestamp PartitionKeys | All current writes hit the "latest" partition, creating a hot partition. Use hash prefix or distributed partition strategy. |

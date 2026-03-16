using Azure.Data.Tables;
using TaskBoard.Api.Contracts;

namespace TaskBoard.Api.Accessors;

/// <summary>
/// VIOLATION FILE
///
/// VIOLATES:
///   TS-001 — Uses a constant PartitionKey ("AuditLog") for all entries.
///            Caps throughput at ~2,000 ops/sec.
///   TS-002 — GetLogsAsync with null userId queries without PartitionKey,
///            causing a full table scan.
///   TS-003 — Does not handle continuation tokens; assumes all results
///            are returned in a single page (max 1,000).
/// </summary>
public class AuditLogAccessor : IAuditLogAccessor
{
    private readonly TableClient _tableClient;

    public AuditLogAccessor(TableClient tableClient)
    {
        _tableClient = tableClient;
    }

    public async Task LogActionAsync(string userId, string action, string entityId, string details)
    {
        // VIOLATION TS-001: Constant PartitionKey — all entries share "AuditLog"
        // Should use userId or a composite key for distribution
        var entity = new TableEntity("AuditLog", Guid.NewGuid().ToString())
        {
            { "UserId", userId },
            { "Action", action },
            { "EntityId", entityId },
            { "Details", details },
            { "Timestamp", DateTime.UtcNow }
        };

        await _tableClient.AddEntityAsync(entity);
    }

    // VIOLATION TS-002: When userId is null, queries without PartitionKey = full table scan
    // VIOLATION TS-003: No continuation token handling — silently drops results after 1,000
    public async Task<IReadOnlyList<AuditLogEntry>> GetLogsAsync(string? userId = null)
    {
        var results = new List<AuditLogEntry>();

        // VIOLATION TS-002: No PartitionKey filter when userId is null
        var query = userId is not null
            ? _tableClient.QueryAsync<TableEntity>(e => e.PartitionKey == "AuditLog" && e.GetString("UserId") == userId)
            : _tableClient.QueryAsync<TableEntity>();   // TABLE SCAN!

        // VIOLATION TS-003: Only processes first page of results
        await foreach (var entity in query)
        {
            results.Add(new AuditLogEntry(
                entity.GetString("UserId"),
                entity.GetString("Action"),
                entity.GetString("EntityId"),
                entity.GetString("Details"),
                entity.GetDateTimeOffset("Timestamp")?.UtcDateTime ?? DateTime.MinValue
            ));

            // Naive "pagination" — just stops at 100, doesn't use continuation tokens
            if (results.Count >= 100) break;
        }

        return results;
    }
}

namespace TaskBoard.Api.Contracts;

public interface IAuditLogAccessor
{
    Task LogActionAsync(string userId, string action, string entityId, string details);
    Task<IReadOnlyList<AuditLogEntry>> GetLogsAsync(string? userId = null);
}

public record AuditLogEntry(
    string UserId,
    string Action,
    string EntityId,
    string Details,
    DateTime Timestamp
);

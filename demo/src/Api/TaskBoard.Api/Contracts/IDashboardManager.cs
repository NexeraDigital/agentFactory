namespace TaskBoard.Api.Contracts;

public interface IDashboardManager
{
    Task<DashboardDto> GetDashboardAsync(string userId, CancellationToken ct);
}

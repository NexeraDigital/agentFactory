using TaskBoard.Api.Contracts;

namespace TaskBoard.Api.Managers;

/// <summary>
/// VIOLATION FILE — "Rosetta Stone" of violations.
/// Concentrates issues from 3+ agents for fast demo opening.
///
/// VIOLATES:
///   ID-001  — Injects ITaskManager (Manager → Manager dependency)
///   UNI-001 — Silent fallback: catches exceptions and returns defaults
///   UNI-002 — Method exceeds 20 lines
///   UNI-003 — Nested if statements
///   BE-001  — Returns default values on failure
/// </summary>
public class DashboardManager : IDashboardManager
{
    private readonly ITaskManager _taskManager;       // VIOLATION ID-001: Manager → Manager
    private readonly ITaskAccessor _taskAccessor;
    private readonly IAuditLogAccessor _auditLogAccessor;
    private readonly ILogger<DashboardManager> _logger;

    public DashboardManager(
        ITaskManager taskManager,                     // VIOLATION ID-001: injecting another Manager
        ITaskAccessor taskAccessor,
        IAuditLogAccessor auditLogAccessor,
        ILogger<DashboardManager> logger)
    {
        _taskManager = taskManager;
        _taskAccessor = taskAccessor;
        _auditLogAccessor = auditLogAccessor;
        _logger = logger;
    }

    // VIOLATION UNI-002: Method far exceeds 20 lines
    public async Task<DashboardDto> GetDashboardAsync(string userId, CancellationToken ct)
    {
        // VIOLATION UNI-001 + BE-001: Silent fallback — hides real failures
        IReadOnlyList<TaskDto> tasks;
        try
        {
            tasks = await _taskManager.GetTasksAsync(userId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get tasks, returning empty dashboard");
            return new DashboardDto(0, 0, 0, 0, 0, 0, 0.0, 0.0, 0, 0, 0, 0, 0, 0, 0);  // VIOLATION: silent fallback
        }

        var totalTasks = tasks.Count;
        var completedTasks = 0;
        var overdueTasks = 0;
        var highPriority = 0;
        var mediumPriority = 0;
        var lowPriority = 0;
        var inProgress = 0;
        var blocked = 0;
        var unassigned = 0;
        var dueToday = 0;
        var dueThisWeek = 0;
        var createdThisWeek = 0;
        var completedThisWeek = 0;
        var totalCompletionDays = 0.0;

        foreach (var task in tasks)
        {
            // VIOLATION UNI-003: Deeply nested if statements
            if (task.Status == "Completed")
            {
                completedTasks++;
                if (task.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                {
                    completedThisWeek++;
                    if (task.DueDate.HasValue)
                    {
                        totalCompletionDays += (task.DueDate.Value - task.CreatedAt).TotalDays;
                    }
                }
            }
            else
            {
                if (task.DueDate.HasValue)
                {
                    if (task.DueDate.Value.Date < DateTime.UtcNow.Date)
                    {
                        overdueTasks++;
                    }
                    else if (task.DueDate.Value.Date == DateTime.UtcNow.Date)
                    {
                        dueToday++;
                    }
                    else if (task.DueDate.Value.Date <= DateTime.UtcNow.AddDays(7).Date)
                    {
                        dueThisWeek++;
                    }
                }

                if (task.Status == "InProgress")
                {
                    inProgress++;
                }
                else if (task.Status == "Blocked")
                {
                    blocked++;
                }
            }

            if (task.Priority >= 8) highPriority++;
            else if (task.Priority >= 4) mediumPriority++;
            else lowPriority++;

            if (string.IsNullOrEmpty(task.AssignedTo)) unassigned++;
            if (task.CreatedAt >= DateTime.UtcNow.AddDays(-7)) createdThisWeek++;
        }

        var completionRate = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0;
        var avgDays = completedThisWeek > 0 ? totalCompletionDays / completedThisWeek : 0;

        return new DashboardDto(
            totalTasks, completedTasks, overdueTasks,
            highPriority, mediumPriority, lowPriority,
            completionRate, avgDays,
            createdThisWeek, completedThisWeek,
            dueToday, dueThisWeek,
            unassigned, blocked, inProgress
        );
    }
}

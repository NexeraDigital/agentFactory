namespace TaskBoard.Api.Contracts;

public record DashboardDto(
    int TotalTasks,
    int CompletedTasks,
    int OverdueTasks,
    int HighPriorityCount,
    int MediumPriorityCount,
    int LowPriorityCount,
    double CompletionRate,
    double AverageCompletionDays,
    int TasksCreatedThisWeek,
    int TasksCompletedThisWeek,
    int TasksDueToday,
    int TasksDueThisWeek,
    int UnassignedTasks,
    int BlockedTasks,
    int InProgressTasks
);

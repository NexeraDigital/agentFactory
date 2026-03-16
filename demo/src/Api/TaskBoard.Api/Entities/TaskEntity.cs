namespace TaskBoard.Api.Entities;

/// <summary>
/// VIOLATION FILE
///
/// VIOLATES:
///   BE-004 — Blocks on async code using .Result (deadlock risk).
///            Should use async/await consistently.
/// </summary>
public class TaskEntity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Todo";
    public int Priority { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }

    // VIOLATION BE-004: Blocking on async — deadlock risk
    // This should be an async method, but blocks synchronously
    public string GetFormattedCreatedDate()
    {
        var formatTask = Task.Run(() => FormatDateAsync(CreatedAt));
        return formatTask.Result;  // VIOLATION: .Result blocks the thread
    }

    private static async Task<string> FormatDateAsync(DateTime date)
    {
        await Task.Delay(1); // simulates async work
        return date.ToString("yyyy-MM-dd HH:mm:ss");
    }
}

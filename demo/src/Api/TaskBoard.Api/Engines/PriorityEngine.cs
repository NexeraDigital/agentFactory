using TaskBoard.Api.Contracts;

namespace TaskBoard.Api.Engines;

/// <summary>
/// VIOLATION FILE
///
/// VIOLATES:
///   ID-002 — Engine injects another Engine (IDeadlineEngine)
///            Engines must contain pure domain logic; if two engines
///            need to collaborate, a Manager coordinates them.
/// </summary>
public class PriorityEngine : IPriorityEngine
{
    private readonly IDeadlineEngine _deadlineEngine;  // VIOLATION ID-002: Engine → Engine

    public PriorityEngine(IDeadlineEngine deadlineEngine)
    {
        _deadlineEngine = deadlineEngine;
    }

    public int CalculatePriority(string status, DateTime? dueDate, int basePriority)
    {
        if (status == "Blocked") return 10;

        var urgencyBoost = _deadlineEngine.CalculateUrgency(dueDate);  // VIOLATION: calling another engine
        return Math.Clamp(basePriority + urgencyBoost, 1, 10);
    }

    public bool IsOverdue(DateTime? dueDate)
    {
        if (!dueDate.HasValue) return false;
        return dueDate.Value.Date < DateTime.UtcNow.Date;
    }
}

/// <summary>
/// Secondary engine that PriorityEngine incorrectly depends on.
/// In correct IDesign, a Manager would coordinate these two engines.
/// </summary>
public interface IDeadlineEngine
{
    int CalculateUrgency(DateTime? dueDate);
}

public class DeadlineEngine : IDeadlineEngine
{
    public int CalculateUrgency(DateTime? dueDate)
    {
        if (!dueDate.HasValue) return 0;

        var daysUntilDue = (dueDate.Value.Date - DateTime.UtcNow.Date).TotalDays;
        return daysUntilDue switch
        {
            < 0 => 5,
            < 1 => 4,
            < 3 => 3,
            < 7 => 2,
            _ => 0
        };
    }
}

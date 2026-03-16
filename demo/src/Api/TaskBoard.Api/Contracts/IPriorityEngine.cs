namespace TaskBoard.Api.Contracts;

public interface IPriorityEngine
{
    int CalculatePriority(string status, DateTime? dueDate, int basePriority);
    bool IsOverdue(DateTime? dueDate);
}

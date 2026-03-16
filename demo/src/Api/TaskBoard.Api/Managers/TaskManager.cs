using TaskBoard.Api.Contracts;

namespace TaskBoard.Api.Managers;

/// <summary>
/// CORRECT IDesign Manager — clean use-case orchestration.
/// Single Manager per use case, dependencies flow downward only,
/// coordinates Engine + Accessor without leaking entities.
/// </summary>
public class TaskManager : ITaskManager
{
    private readonly IPriorityEngine _priorityEngine;
    private readonly ITaskAccessor _taskAccessor;
    private readonly IAuditLogAccessor _auditLogAccessor;
    private readonly ILogger<TaskManager> _logger;

    public TaskManager(
        IPriorityEngine priorityEngine,
        ITaskAccessor taskAccessor,
        IAuditLogAccessor auditLogAccessor,
        ILogger<TaskManager> logger)
    {
        _priorityEngine = priorityEngine;
        _taskAccessor = taskAccessor;
        _auditLogAccessor = auditLogAccessor;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TaskDto>> GetTasksAsync(string userId, CancellationToken ct)
    {
        var tasks = await _taskAccessor.GetTasksByUserAsync(userId, ct);
        _logger.LogInformation("Retrieved {Count} tasks for user {UserId}", tasks.Count, userId);
        return tasks;
    }

    public async Task<TaskDto> GetTaskByIdAsync(string userId, int taskId, CancellationToken ct)
    {
        var task = await _taskAccessor.GetTaskByIdAsync(userId, taskId, ct);
        if (task is null)
            throw new KeyNotFoundException($"Task {taskId} not found for user {userId}");
        return task;
    }

    public async Task<TaskDto> CreateTaskAsync(string userId, CreateTaskRequest request, CancellationToken ct)
    {
        var effectivePriority = _priorityEngine.CalculatePriority("Todo", request.DueDate, request.Priority);
        var enrichedRequest = request with { Priority = effectivePriority };

        var task = await _taskAccessor.CreateTaskAsync(userId, enrichedRequest, ct);
        await _auditLogAccessor.LogActionAsync(userId, "TaskCreated", task.Id.ToString(), $"Created: {task.Title}");
        return task;
    }

    public async Task<TaskDto> UpdateTaskAsync(string userId, int taskId, UpdateTaskRequest request, CancellationToken ct)
    {
        await GetTaskByIdAsync(userId, taskId, ct);
        var task = await _taskAccessor.UpdateTaskAsync(userId, taskId, request, ct);
        await _auditLogAccessor.LogActionAsync(userId, "TaskUpdated", taskId.ToString(), $"Updated: {task.Title}");
        return task;
    }

    public async Task DeleteTaskAsync(string userId, int taskId, CancellationToken ct)
    {
        await GetTaskByIdAsync(userId, taskId, ct);
        await _taskAccessor.DeleteTaskAsync(userId, taskId, ct);
        await _auditLogAccessor.LogActionAsync(userId, "TaskDeleted", taskId.ToString(), $"Deleted task {taskId}");
    }
}

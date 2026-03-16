namespace TaskBoard.Api.Contracts;

public interface ITaskManager
{
    Task<IReadOnlyList<TaskDto>> GetTasksAsync(string userId, CancellationToken ct);
    Task<TaskDto> GetTaskByIdAsync(string userId, int taskId, CancellationToken ct);
    Task<TaskDto> CreateTaskAsync(string userId, CreateTaskRequest request, CancellationToken ct);
    Task<TaskDto> UpdateTaskAsync(string userId, int taskId, UpdateTaskRequest request, CancellationToken ct);
    Task DeleteTaskAsync(string userId, int taskId, CancellationToken ct);
}

namespace TaskBoard.Api.Contracts;

public interface ITaskAccessor
{
    Task<IReadOnlyList<TaskDto>> GetTasksByUserAsync(string userId, CancellationToken ct);
    Task<TaskDto?> GetTaskByIdAsync(int taskId, CancellationToken ct);
    Task<TaskDto> CreateTaskAsync(string userId, CreateTaskRequest request, CancellationToken ct);
    Task<TaskDto> UpdateTaskAsync(int taskId, UpdateTaskRequest request, CancellationToken ct);
    Task DeleteTaskAsync(int taskId, CancellationToken ct);
}

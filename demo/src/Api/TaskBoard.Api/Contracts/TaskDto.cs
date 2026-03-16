namespace TaskBoard.Api.Contracts;

public record TaskDto(
    int Id,
    string Title,
    string Description,
    string Status,
    int Priority,
    string AssignedTo,
    DateTime CreatedAt,
    DateTime? DueDate
);

public record CreateTaskRequest(
    string Title,
    string Description,
    int Priority,
    DateTime? DueDate
);

public record UpdateTaskRequest(
    string Title,
    string Description,
    string Status,
    int Priority,
    DateTime? DueDate
);

using Microsoft.EntityFrameworkCore;
using TaskBoard.Api.Contracts;
using TaskBoard.Api.Entities;

namespace TaskBoard.Api.Accessors;

public class TaskAccessor : ITaskAccessor
{
    private readonly TaskDbContext _db;

    public TaskAccessor(TaskDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<TaskDto>> GetTasksByUserAsync(string userId, CancellationToken ct)
    {
        // VIOLATION SQL-005: Missing AsNoTracking() for read-only query
        // VIOLATION SQL-007: Loads full entity, then maps — should use .Select() projection
        var entities = await _db.Tasks
            .Where(t => t.UserId == userId)
            .ToListAsync(ct);
            

        return entities.Select(e => new TaskDto(
            e.Id, e.Title, e.Description, e.Status,
            e.Priority, e.AssignedTo, e.CreatedAt, e.DueDate
        )).ToList();
    }

    public async Task<TaskDto?> GetTaskByIdAsync(string userId, int taskId, CancellationToken ct)
    {
        // VIOLATION SQL-005: Missing AsNoTracking()
        var entity = await _db.Tasks
            .Where(t => t.UserId == userId && t.Id == taskId)
            .FirstOrDefaultAsync(ct);
        if (entity is null) return null;

        return new TaskDto(
            entity.Id, entity.Title, entity.Description, entity.Status,
            entity.Priority, entity.AssignedTo, entity.CreatedAt, entity.DueDate
        );
    }

    public async Task<TaskDto> CreateTaskAsync(string userId, CreateTaskRequest request, CancellationToken ct)
    {
        var entity = new TaskEntity
        {
            Title = request.Title,
            Description = request.Description,
            Status = "Todo",
            Priority = request.Priority,
            UserId = userId,
            AssignedTo = userId,
            CreatedAt = DateTime.UtcNow,
            DueDate = request.DueDate
        };

        _db.Tasks.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new TaskDto(
            entity.Id, entity.Title, entity.Description, entity.Status,
            entity.Priority, entity.AssignedTo, entity.CreatedAt, entity.DueDate
        );
    }

    public async Task<TaskDto> UpdateTaskAsync(string userId, int taskId, UpdateTaskRequest request, CancellationToken ct)
    {
        var entity = await _db.Tasks
            .Where(t => t.UserId == userId && t.Id == taskId)
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException($"Task {taskId} not found for user {userId}");

        entity.Title = request.Title;
        entity.Description = request.Description;
        entity.Status = request.Status;
        entity.Priority = request.Priority;
        entity.DueDate = request.DueDate;

        await _db.SaveChangesAsync(ct);

        return new TaskDto(
            entity.Id, entity.Title, entity.Description, entity.Status,
            entity.Priority, entity.AssignedTo, entity.CreatedAt, entity.DueDate
        );
    }

    public async Task DeleteTaskAsync(string userId, int taskId, CancellationToken ct)
    {
        var entity = await _db.Tasks
            .Where(t => t.UserId == userId && t.Id == taskId)
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException($"Task {taskId} not found for user {userId}");

        _db.Tasks.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    // VIOLATION SQL-006: OFFSET/FETCH pagination — degrades linearly at high page numbers.
    // Should use keyset pagination: WHERE Id > @lastId ORDER BY Id
    public async Task<IReadOnlyList<TaskDto>> GetTasksPagedAsync(
        string userId, int page, int pageSize, CancellationToken ct)
    {
        var entities = await _db.Tasks
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.Id)
            .Skip((page - 1) * pageSize)           // VIOLATION SQL-006
            .Take(pageSize)
            .ToListAsync(ct);

        return entities.Select(e => new TaskDto(
            e.Id, e.Title, e.Description, e.Status,
            e.Priority, e.AssignedTo, e.CreatedAt, e.DueDate
        )).ToList();
    }
}

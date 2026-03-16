using Microsoft.EntityFrameworkCore;
using TaskBoard.Api.Contracts;
using TaskBoard.Api.Entities;

namespace TaskBoard.Api.Accessors;

/// <summary>
/// VIOLATION FILE
///
/// VIOLATES:
///   SEC-001 — GetTaskByIdAsync does NOT scope by user; any user can
///             retrieve any task by guessing the ID (BOLA/IDOR).
///   SQL-005 — Read queries do NOT use AsNoTracking().
///   SQL-006 — Uses OFFSET/FETCH pagination (degrades at high page numbers).
///   SQL-007 — Returns full entities instead of projecting to DTOs.
///   ID-007  — Leaks EF entities across layer boundaries.
/// </summary>
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

    // VIOLATION SEC-001: No user scoping — any authenticated user can
    // access any task by ID. This is a BOLA/IDOR vulnerability.
    public async Task<TaskDto?> GetTaskByIdAsync(int taskId, CancellationToken ct)
    {
        // VIOLATION SQL-005: Missing AsNoTracking()
        var entity = await _db.Tasks.FindAsync(new object[] { taskId }, ct);
        if (entity is null) return null;

        // VIOLATION ID-007: Entity leaked across boundary (converted here but
        // the interface design encourages passing entities)
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

    public async Task<TaskDto> UpdateTaskAsync(int taskId, UpdateTaskRequest request, CancellationToken ct)
    {
        // VIOLATION SEC-001: No user scoping on update
        var entity = await _db.Tasks.FindAsync(new object[] { taskId }, ct)
            ?? throw new KeyNotFoundException($"Task {taskId} not found");

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

    public async Task DeleteTaskAsync(int taskId, CancellationToken ct)
    {
        // VIOLATION SEC-001: No user scoping on delete
        var entity = await _db.Tasks.FindAsync(new object[] { taskId }, ct)
            ?? throw new KeyNotFoundException($"Task {taskId} not found");

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

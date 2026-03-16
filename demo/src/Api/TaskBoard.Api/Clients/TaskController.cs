using Microsoft.AspNetCore.Mvc;
using TaskBoard.Api.Contracts;

namespace TaskBoard.Api.Clients;

/// <summary>
/// VIOLATION FILE
///
/// VIOLATES:
///   ID-004 — Calls two Managers from one Client endpoint.
///            Clients must call exactly ONE Manager per use case.
///            The dashboard endpoint should use only IDashboardManager.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{
    private readonly ITaskManager _taskManager;
    private readonly IDashboardManager _dashboardManager;  // VIOLATION ID-004: second Manager

    public TaskController(
        ITaskManager taskManager,
        IDashboardManager dashboardManager)                // VIOLATION ID-004
    {
        _taskManager = taskManager;
        _dashboardManager = dashboardManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks(CancellationToken ct)
    {
        var userId = User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");
        var tasks = await _taskManager.GetTasksAsync(userId, ct);
        return Ok(tasks);
    }

    [HttpGet("{taskId:int}")]
    public async Task<IActionResult> GetTask(int taskId, CancellationToken ct)
    {
        var userId = User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");
        var task = await _taskManager.GetTaskByIdAsync(userId, taskId, ct);
        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        var userId = User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");
        var task = await _taskManager.CreateTaskAsync(userId, request, ct);
        return CreatedAtAction(nameof(GetTask), new { taskId = task.Id }, task);
    }

    [HttpPut("{taskId:int}")]
    public async Task<IActionResult> UpdateTask(int taskId, [FromBody] UpdateTaskRequest request, CancellationToken ct)
    {
        var userId = User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");
        var task = await _taskManager.UpdateTaskAsync(userId, taskId, request, ct);
        return Ok(task);
    }

    [HttpDelete("{taskId:int}")]
    public async Task<IActionResult> DeleteTask(int taskId, CancellationToken ct)
    {
        var userId = User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");
        await _taskManager.DeleteTaskAsync(userId, taskId, ct);
        return NoContent();
    }

    // VIOLATION ID-004: This endpoint calls _dashboardManager,
    // but the controller already injects _taskManager.
    // Should be a separate DashboardController.
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var userId = User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");
        var dashboard = await _dashboardManager.GetDashboardAsync(userId, ct);
        return Ok(dashboard);
    }
}

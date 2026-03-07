using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSaaS.Application.Tasks;
using ProjectManagementSaaS.Domain.Enums;

namespace ProjectManagementSaaS.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tasks")]
[Authorize]
public sealed class TasksController(ITaskService taskService) : ControllerBase
{
    [HttpGet("{taskId:guid}")]
    public async Task<ActionResult<TaskResponse>> Get(Guid taskId, CancellationToken cancellationToken) =>
        Ok(await taskService.GetTaskAsync(taskId, cancellationToken));

    [HttpPost]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Manager)}")]
    public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var response = await taskService.CreateTaskAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPut("{taskId:guid}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Manager)}")]
    public async Task<ActionResult<TaskResponse>> Update(Guid taskId, UpdateTaskRequest request, CancellationToken cancellationToken) =>
        Ok(await taskService.UpdateTaskAsync(taskId, request, cancellationToken));

    [HttpPatch("{taskId:guid}/status")]
    public async Task<ActionResult<TaskResponse>> UpdateStatus(Guid taskId, UpdateTaskStatusRequest request, CancellationToken cancellationToken) =>
        Ok(await taskService.UpdateTaskStatusAsync(taskId, request, cancellationToken));

    [HttpPost("{taskId:guid}/comments")]
    public async Task<ActionResult<TaskCommentResponse>> AddComment(Guid taskId, AddTaskCommentRequest request, CancellationToken cancellationToken) =>
        Ok(await taskService.AddCommentAsync(taskId, request, cancellationToken));

    [HttpDelete("{taskId:guid}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> Delete(Guid taskId, CancellationToken cancellationToken)
    {
        await taskService.DeleteTaskAsync(taskId, cancellationToken);
        return NoContent();
    }
}

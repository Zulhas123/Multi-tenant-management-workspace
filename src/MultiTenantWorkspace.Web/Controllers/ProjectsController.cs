using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSaaS.Application.Projects;
using ProjectManagementSaaS.Domain.Enums;

namespace ProjectManagementSaaS.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/projects")]
[Authorize]
public sealed class ProjectsController(IProjectService projectService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProjectResponse>>> GetProjects(CancellationToken cancellationToken) =>
        Ok(await projectService.GetProjectsAsync(cancellationToken));

    [HttpPost]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Manager)}")]
    public async Task<ActionResult<ProjectResponse>> Create(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var response = await projectService.CreateProjectAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetProjects), new { id = response.ProjectId, version = "1" }, response);
    }

    [HttpPut("{projectId:guid}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Manager)}")]
    public async Task<ActionResult<ProjectResponse>> Update(Guid projectId, UpdateProjectRequest request, CancellationToken cancellationToken) =>
        Ok(await projectService.UpdateProjectAsync(projectId, request, cancellationToken));

    [HttpDelete("{projectId:guid}")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Manager)}")]
    public async Task<IActionResult> Delete(Guid projectId, CancellationToken cancellationToken)
    {
        await projectService.DeleteProjectAsync(projectId, cancellationToken);
        return NoContent();
    }
}

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSaaS.Application.Organizations;
using ProjectManagementSaaS.Domain.Enums;

namespace ProjectManagementSaaS.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/organization")]
[Authorize]
public sealed class OrganizationsController(IOrganizationService organizationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<OrganizationSummaryResponse>> GetCurrent(CancellationToken cancellationToken) =>
        Ok(await organizationService.GetCurrentOrganizationAsync(cancellationToken));

    [HttpGet("users")]
    [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Manager)}")]
    public async Task<ActionResult<IReadOnlyCollection<OrganizationUserResponse>>> GetUsers(CancellationToken cancellationToken) =>
        Ok(await organizationService.GetOrganizationUsersAsync(cancellationToken));

    [HttpGet("activity-logs")]
    public async Task<ActionResult<IReadOnlyCollection<ActivityLogResponse>>> GetActivityLogs(CancellationToken cancellationToken) =>
        Ok(await organizationService.GetActivityLogsAsync(cancellationToken));
}

using ProjectManagementSaaS.Domain.Enums;

namespace ProjectManagementSaaS.Application.Projects;

public sealed record UpdateProjectRequest(
    string Name,
    string? Description,
    ProjectStatus Status,
    DateTime? StartDateUtc,
    DateTime? DueDateUtc);

using ProjectManagementSaaS.Domain.Enums;

namespace ProjectManagementSaaS.Application.Projects;

public sealed record CreateProjectRequest(
    string Name,
    string? Description,
    ProjectStatus Status,
    DateTime? StartDateUtc,
    DateTime? DueDateUtc);

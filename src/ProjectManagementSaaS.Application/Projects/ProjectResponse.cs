using ProjectManagementSaaS.Domain.Enums;

namespace ProjectManagementSaaS.Application.Projects;

public sealed record ProjectResponse(
    Guid ProjectId,
    string Name,
    string? Description,
    ProjectStatus Status,
    DateTime? StartDateUtc,
    DateTime? DueDateUtc,
    IReadOnlyCollection<ProjectTaskSummaryResponse> Tasks);

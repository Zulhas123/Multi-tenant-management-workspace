using ProjectManagementSaaS.Domain.Enums;
using DomainTaskStatus = ProjectManagementSaaS.Domain.Enums.TaskStatus;

namespace ProjectManagementSaaS.Application.Projects;

public sealed record ProjectTaskSummaryResponse(
    Guid TaskId,
    string Title,
    DomainTaskStatus Status,
    string? AssigneeName,
    DateTime? DueDateUtc);

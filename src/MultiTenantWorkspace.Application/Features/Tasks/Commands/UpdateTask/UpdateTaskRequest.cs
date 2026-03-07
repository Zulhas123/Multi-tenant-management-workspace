using ProjectManagementSaaS.Domain.Enums;
using DomainTaskStatus = ProjectManagementSaaS.Domain.Enums.TaskStatus;

namespace ProjectManagementSaaS.Application.Tasks;

public sealed record UpdateTaskRequest(
    Guid ProjectId,
    string Title,
    string? Description,
    Guid? AssignedToUserId,
    TaskPriority Priority,
    DomainTaskStatus Status,
    DateTime? DueDateUtc);

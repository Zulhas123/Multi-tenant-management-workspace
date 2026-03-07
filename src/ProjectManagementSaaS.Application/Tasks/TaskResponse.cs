using ProjectManagementSaaS.Domain.Enums;
using DomainTaskStatus = ProjectManagementSaaS.Domain.Enums.TaskStatus;

namespace ProjectManagementSaaS.Application.Tasks;

public sealed record TaskResponse(
    Guid TaskId,
    Guid ProjectId,
    string Title,
    string? Description,
    Guid? AssignedToUserId,
    string? AssigneeName,
    DomainTaskStatus Status,
    TaskPriority Priority,
    DateTime? DueDateUtc,
    IReadOnlyCollection<TaskCommentResponse> Comments);

using ProjectManagementSaaS.Domain.Enums;

namespace ProjectManagementSaaS.Application.Tasks;

public sealed record CreateTaskRequest(
    Guid ProjectId,
    string Title,
    string? Description,
    Guid? AssignedToUserId,
    TaskPriority Priority,
    DateTime? DueDateUtc);

using ProjectManagementSaaS.Domain.Enums;
using DomainTaskStatus = ProjectManagementSaaS.Domain.Enums.TaskStatus;

namespace ProjectManagementSaaS.Application.Tasks;

public sealed record UpdateTaskStatusRequest(DomainTaskStatus Status);

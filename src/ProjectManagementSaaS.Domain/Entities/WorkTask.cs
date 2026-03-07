using ProjectManagementSaaS.Domain.Common;
using ProjectManagementSaaS.Domain.Enums;
using DomainTaskStatus = ProjectManagementSaaS.Domain.Enums.TaskStatus;

namespace ProjectManagementSaaS.Domain.Entities;

public sealed class WorkTask : BaseAuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public Guid? AssignedToUserId { get; set; }
    public ApplicationUser? AssignedToUser { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DomainTaskStatus Status { get; set; } = DomainTaskStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDateUtc { get; set; }
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}

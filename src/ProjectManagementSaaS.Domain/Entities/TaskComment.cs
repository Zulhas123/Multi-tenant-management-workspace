using ProjectManagementSaaS.Domain.Common;

namespace ProjectManagementSaaS.Domain.Entities;

public sealed class TaskComment : BaseAuditableEntity
{
    public Guid TaskId { get; set; }
    public WorkTask Task { get; set; } = null!;
    public Guid AuthorUserId { get; set; }
    public ApplicationUser AuthorUser { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
}

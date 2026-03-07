using ProjectManagementSaaS.Domain.Common;
using ProjectManagementSaaS.Domain.Enums;

namespace ProjectManagementSaaS.Domain.Entities;

public sealed class Project : BaseAuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public Guid CreatedByUserId { get; set; }
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planned;
    public DateTime? StartDateUtc { get; set; }
    public DateTime? DueDateUtc { get; set; }
    public ICollection<WorkTask> Tasks { get; set; } = new List<WorkTask>();
}

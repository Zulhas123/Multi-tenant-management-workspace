using ProjectManagementSaaS.Domain.Common;

namespace ProjectManagementSaaS.Domain.Entities;

public sealed class Organization : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
}

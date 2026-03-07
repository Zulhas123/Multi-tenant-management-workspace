using ProjectManagementSaaS.Domain.Common;
using ProjectManagementSaaS.Domain.Enums;

namespace ProjectManagementSaaS.Domain.Entities;

public sealed class ApplicationUser : BaseAuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Member;
    public ICollection<Project> CreatedProjects { get; set; } = new List<Project>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<WorkTask> AssignedTasks { get; set; } = new List<WorkTask>();
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}

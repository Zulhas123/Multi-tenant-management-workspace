using ProjectManagementSaaS.Domain.Common;

namespace ProjectManagementSaaS.Domain.Entities;

public sealed class RefreshToken : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresUtc { get; set; }
    public DateTime? RevokedUtc { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public string? ReasonRevoked { get; set; }

    public bool IsActive(DateTime utcNow) => RevokedUtc is null && ExpiresUtc > utcNow;
}

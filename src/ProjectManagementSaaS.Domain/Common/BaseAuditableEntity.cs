namespace ProjectManagementSaaS.Domain.Common;

public abstract class BaseAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public Guid? CreatedById { get; set; }
    public DateTime? UpdatedUtc { get; set; }
    public Guid? UpdatedById { get; set; }
}

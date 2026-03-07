using ProjectManagementSaaS.Domain.Enums;

namespace ProjectManagementSaaS.Application.Common.Security;

public sealed record CurrentUser(Guid UserId, Guid OrganizationId, string Email, UserRole Role)
{
    public bool IsAdminOrManager => Role is UserRole.Admin or UserRole.Manager;
}

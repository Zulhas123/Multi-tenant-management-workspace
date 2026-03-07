using ProjectManagementSaaS.Domain.Enums;

namespace ProjectManagementSaaS.Application.Organizations;

public sealed record OrganizationUserResponse(Guid UserId, string FullName, string Email, UserRole Role);

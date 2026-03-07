using ProjectManagementSaaS.Domain.Enums;

namespace ProjectManagementSaaS.Application.Auth;

public sealed record AuthResponse(
    string Token,
    string RefreshToken,
    DateTime RefreshTokenExpiresUtc,
    Guid UserId,
    Guid OrganizationId,
    string FullName,
    string Email,
    UserRole Role);

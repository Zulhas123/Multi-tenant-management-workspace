using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ProjectManagementSaaS.Application.Common.Exceptions;
using ProjectManagementSaaS.Application.Common.Interfaces;
using ProjectManagementSaaS.Application.Common.Security;
using ProjectManagementSaaS.Domain.Enums;

namespace ProjectManagementSaaS.Api.Security;

public sealed class HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public CurrentUser GetRequiredUser()
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            throw new ForbiddenException("Authentication is required.");
        }

        var userId = GetClaim(user, JwtRegisteredClaimNames.Sub);
        var organizationId = GetClaim(user, "organizationId");
        var email = GetClaim(user, ClaimTypes.Email);
        var role = GetClaim(user, ClaimTypes.Role);

        return new CurrentUser(
            Guid.Parse(userId),
            Guid.Parse(organizationId),
            email,
            Enum.Parse<UserRole>(role, ignoreCase: true));
    }

    private static string GetClaim(ClaimsPrincipal user, string claimType) =>
        user.FindFirstValue(claimType) ?? throw new ForbiddenException($"Missing claim '{claimType}'.");
}

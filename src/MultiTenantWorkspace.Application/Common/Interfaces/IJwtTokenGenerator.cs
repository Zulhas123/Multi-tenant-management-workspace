using ProjectManagementSaaS.Domain.Entities;

namespace ProjectManagementSaaS.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser user);
}

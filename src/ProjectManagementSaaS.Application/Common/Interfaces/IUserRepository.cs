using ProjectManagementSaaS.Domain.Entities;

namespace ProjectManagementSaaS.Application.Common.Interfaces;

public interface IUserRepository
{
    Task AddAsync(ApplicationUser user, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<ApplicationUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<ApplicationUser?> GetByIdWithRefreshTokensAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ApplicationUser>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken);
}

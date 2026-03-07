using ProjectManagementSaaS.Domain.Entities;

namespace ProjectManagementSaaS.Application.Common.Interfaces;

public interface IOrganizationRepository
{
    Task AddAsync(Organization organization, CancellationToken cancellationToken);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken);
    Task<Organization?> GetByIdAsync(Guid organizationId, CancellationToken cancellationToken);
}

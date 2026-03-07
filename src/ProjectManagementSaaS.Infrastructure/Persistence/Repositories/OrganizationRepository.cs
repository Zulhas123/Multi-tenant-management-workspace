using Microsoft.EntityFrameworkCore;
using ProjectManagementSaaS.Application.Common.Interfaces;
using ProjectManagementSaaS.Domain.Entities;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Repositories;

public sealed class OrganizationRepository(ApplicationDbContext dbContext) : IOrganizationRepository
{
    public Task AddAsync(Organization organization, CancellationToken cancellationToken) =>
        dbContext.Organizations.AddAsync(organization, cancellationToken).AsTask();

    public Task<Organization?> GetByIdAsync(Guid organizationId, CancellationToken cancellationToken) =>
        dbContext.Organizations.FirstOrDefaultAsync(x => x.Id == organizationId, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken) =>
        dbContext.Organizations.AnyAsync(x => x.Slug == slug, cancellationToken);
}

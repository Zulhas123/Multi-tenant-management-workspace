using Microsoft.EntityFrameworkCore;
using ProjectManagementSaaS.Application.Common.Interfaces;
using ProjectManagementSaaS.Domain.Entities;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(ApplicationDbContext dbContext) : IUserRepository
{
    public Task AddAsync(ApplicationUser user, CancellationToken cancellationToken) =>
        dbContext.Users.AddAsync(user, cancellationToken).AsTask();

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) =>
        dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);

    public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

    public Task<ApplicationUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

    public Task<ApplicationUser?> GetByIdWithRefreshTokensAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.Users
            .Include(x => x.RefreshTokens)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

    public async Task<IReadOnlyCollection<ApplicationUser>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken) =>
        await dbContext.Users
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.FullName)
            .ToArrayAsync(cancellationToken);
}

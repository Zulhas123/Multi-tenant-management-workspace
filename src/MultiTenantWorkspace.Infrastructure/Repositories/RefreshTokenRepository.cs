using Microsoft.EntityFrameworkCore;
using ProjectManagementSaaS.Application.Common.Interfaces;
using ProjectManagementSaaS.Domain.Entities;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository(ApplicationDbContext dbContext) : IRefreshTokenRepository
{
    public Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken) =>
        dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken).AsTask();

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
}

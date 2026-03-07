using Microsoft.EntityFrameworkCore;
using ProjectManagementSaaS.Application.Common.Interfaces;
using ProjectManagementSaaS.Domain.Entities;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Repositories;

public sealed class ActivityLogRepository(ApplicationDbContext dbContext) : IActivityLogRepository
{
    public Task AddAsync(ActivityLog activityLog, CancellationToken cancellationToken) =>
        dbContext.ActivityLogs.AddAsync(activityLog, cancellationToken).AsTask();

    public async Task<IReadOnlyCollection<ActivityLog>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken) =>
        await dbContext.ActivityLogs
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.CreatedUtc)
            .ToArrayAsync(cancellationToken);
}

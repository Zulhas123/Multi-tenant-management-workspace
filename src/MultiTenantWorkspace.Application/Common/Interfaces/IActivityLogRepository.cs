using ProjectManagementSaaS.Domain.Entities;

namespace ProjectManagementSaaS.Application.Common.Interfaces;

public interface IActivityLogRepository
{
    Task AddAsync(ActivityLog activityLog, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ActivityLog>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken);
}

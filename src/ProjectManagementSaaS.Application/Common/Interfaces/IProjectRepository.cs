using ProjectManagementSaaS.Domain.Entities;

namespace ProjectManagementSaaS.Application.Common.Interfaces;

public interface IProjectRepository
{
    Task AddAsync(Project project, CancellationToken cancellationToken);
    Task<Project?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Project>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken);
}

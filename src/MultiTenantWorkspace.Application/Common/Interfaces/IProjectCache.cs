using ProjectManagementSaaS.Application.Projects;

namespace ProjectManagementSaaS.Application.Common.Interfaces;

public interface IProjectCache
{
    Task<IReadOnlyCollection<ProjectResponse>?> GetOrganizationProjectsAsync(Guid organizationId, CancellationToken cancellationToken);
    Task SetOrganizationProjectsAsync(Guid organizationId, IReadOnlyCollection<ProjectResponse> projects, CancellationToken cancellationToken);
    Task InvalidateOrganizationProjectsAsync(Guid organizationId, CancellationToken cancellationToken);
}

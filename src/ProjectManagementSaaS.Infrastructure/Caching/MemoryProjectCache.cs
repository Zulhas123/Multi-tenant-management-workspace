using Microsoft.Extensions.Caching.Memory;
using ProjectManagementSaaS.Application.Common.Interfaces;
using ProjectManagementSaaS.Application.Projects;

namespace ProjectManagementSaaS.Infrastructure.Caching;

public sealed class MemoryProjectCache(IMemoryCache memoryCache) : IProjectCache
{
    public Task<IReadOnlyCollection<ProjectResponse>?> GetOrganizationProjectsAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        memoryCache.TryGetValue(GetKey(organizationId), out IReadOnlyCollection<ProjectResponse>? projects);
        return Task.FromResult(projects);
    }

    public Task SetOrganizationProjectsAsync(Guid organizationId, IReadOnlyCollection<ProjectResponse> projects, CancellationToken cancellationToken)
    {
        memoryCache.Set(GetKey(organizationId), projects, TimeSpan.FromMinutes(5));
        return Task.CompletedTask;
    }

    public Task InvalidateOrganizationProjectsAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        memoryCache.Remove(GetKey(organizationId));
        return Task.CompletedTask;
    }

    private static string GetKey(Guid organizationId) => $"org-projects:{organizationId}";
}

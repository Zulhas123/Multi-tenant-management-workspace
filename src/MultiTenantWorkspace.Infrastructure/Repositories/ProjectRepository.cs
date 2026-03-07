using Microsoft.EntityFrameworkCore;
using ProjectManagementSaaS.Application.Common.Interfaces;
using ProjectManagementSaaS.Domain.Entities;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Repositories;

public sealed class ProjectRepository(ApplicationDbContext dbContext) : IProjectRepository
{
    public Task AddAsync(Project project, CancellationToken cancellationToken) =>
        dbContext.Projects.AddAsync(project, cancellationToken).AsTask();

    public Task<Project?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken) =>
        dbContext.Projects
            .Include(x => x.Tasks)
            .ThenInclude(x => x.AssignedToUser)
            .FirstOrDefaultAsync(x => x.Id == projectId, cancellationToken);

    public async Task<IReadOnlyCollection<Project>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken) =>
        await dbContext.Projects
            .AsNoTracking()
            .Include(x => x.Tasks)
            .ThenInclude(x => x.AssignedToUser)
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.Name)
            .ToArrayAsync(cancellationToken);

    public void Remove(Project project) => dbContext.Projects.Remove(project);
}

using ProjectManagementSaaS.Application.Common.Interfaces;
using ProjectManagementSaaS.Application.Common.Security;
using ProjectManagementSaaS.Application.Projects;
using ProjectManagementSaaS.Domain.Entities;
using ProjectManagementSaaS.Domain.Enums;

namespace ProjectManagementSaaS.Application.Tests;

public sealed class ProjectServiceTests
{
    [Fact]
    public async Task UpdateProjectAsync_ShouldUpdateProjectForManager()
    {
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var project = new Project
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = "Old project",
            Status = ProjectStatus.Planned
        };

        var repository = new FakeProjectRepository(project);
        var service = new ProjectService(
            new FakeCurrentUserService(new CurrentUser(userId, organizationId, "manager@example.com", UserRole.Manager)),
            repository,
            new FakeActivityLogRepository(),
            new FakeProjectCache(),
            new FakeDateTimeProvider(),
            new FakeUnitOfWork());

        var result = await service.UpdateProjectAsync(
            project.Id,
            new UpdateProjectRequest("Updated project", "Updated", ProjectStatus.Active, null, null),
            CancellationToken.None);

        Assert.Equal("Updated project", result.Name);
        Assert.Equal(ProjectStatus.Active, result.Status);
    }

    [Fact]
    public async Task DeleteProjectAsync_ShouldRemoveProject()
    {
        var organizationId = Guid.NewGuid();
        var project = new Project
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = "Delete me"
        };

        var repository = new FakeProjectRepository(project);
        var service = new ProjectService(
            new FakeCurrentUserService(new CurrentUser(Guid.NewGuid(), organizationId, "admin@example.com", UserRole.Admin)),
            repository,
            new FakeActivityLogRepository(),
            new FakeProjectCache(),
            new FakeDateTimeProvider(),
            new FakeUnitOfWork());

        await service.DeleteProjectAsync(project.Id, CancellationToken.None);

        Assert.True(repository.Removed);
    }

    private sealed class FakeCurrentUserService(CurrentUser currentUser) : ICurrentUserService
    {
        public CurrentUser GetRequiredUser() => currentUser;
    }

    private sealed class FakeProjectRepository(Project project) : IProjectRepository
    {
        public bool Removed { get; private set; }

        public Task AddAsync(Project projectToAdd, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<Project?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken) =>
            Task.FromResult<Project?>(projectId == project.Id ? project : null);

        public Task<IReadOnlyCollection<Project>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<Project>>(new[] { project });

        public void Remove(Project projectToRemove)
        {
            if (projectToRemove.Id == project.Id)
            {
                Removed = true;
            }
        }
    }

    private sealed class FakeActivityLogRepository : IActivityLogRepository
    {
        public Task AddAsync(ActivityLog activityLog, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<IReadOnlyCollection<ActivityLog>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<ActivityLog>>(Array.Empty<ActivityLog>());
    }

    private sealed class FakeProjectCache : IProjectCache
    {
        public Task<IReadOnlyCollection<ProjectResponse>?> GetOrganizationProjectsAsync(Guid organizationId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<ProjectResponse>?>(null);

        public Task SetOrganizationProjectsAsync(Guid organizationId, IReadOnlyCollection<ProjectResponse> projects, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task InvalidateOrganizationProjectsAsync(Guid organizationId, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => new(2026, 3, 7, 0, 0, 0, DateTimeKind.Utc);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
    }
}

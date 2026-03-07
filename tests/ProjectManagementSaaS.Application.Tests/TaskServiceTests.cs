using ProjectManagementSaaS.Application.Common.Exceptions;
using ProjectManagementSaaS.Application.Common.Interfaces;
using ProjectManagementSaaS.Application.Common.Security;
using ProjectManagementSaaS.Application.Tasks;
using ProjectManagementSaaS.Domain.Entities;
using ProjectManagementSaaS.Domain.Enums;
using DomainTaskStatus = ProjectManagementSaaS.Domain.Enums.TaskStatus;

namespace ProjectManagementSaaS.Application.Tests;

public sealed class TaskServiceTests
{
    [Fact]
    public async Task UpdateTaskStatusAsync_ShouldAllowAssignedMember()
    {
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var task = new WorkTask
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            ProjectId = Guid.NewGuid(),
            Title = "Initial task",
            AssignedToUserId = userId
        };

        var service = new TaskService(
            new FakeCurrentUserService(new CurrentUser(userId, organizationId, "member@example.com", UserRole.Member)),
            new FakeProjectRepository(),
            new FakeTaskRepository(task),
            new FakeUserRepository(),
            new FakeTaskCommentRepository(),
            new FakeActivityLogRepository(),
            new FakeProjectCache(),
            new FakeDateTimeProvider(),
            new FakeUnitOfWork());

        var result = await service.UpdateTaskStatusAsync(task.Id, new UpdateTaskStatusRequest(DomainTaskStatus.Done), CancellationToken.None);

        Assert.Equal(DomainTaskStatus.Done, result.Status);
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_ShouldRejectDifferentMember()
    {
        var organizationId = Guid.NewGuid();
        var task = new WorkTask
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            ProjectId = Guid.NewGuid(),
            Title = "Initial task",
            AssignedToUserId = Guid.NewGuid()
        };

        var service = new TaskService(
            new FakeCurrentUserService(new CurrentUser(Guid.NewGuid(), organizationId, "member@example.com", UserRole.Member)),
            new FakeProjectRepository(),
            new FakeTaskRepository(task),
            new FakeUserRepository(),
            new FakeTaskCommentRepository(),
            new FakeActivityLogRepository(),
            new FakeProjectCache(),
            new FakeDateTimeProvider(),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.UpdateTaskStatusAsync(task.Id, new UpdateTaskStatusRequest(DomainTaskStatus.Done), CancellationToken.None));
    }

    private sealed class FakeCurrentUserService(CurrentUser currentUser) : ICurrentUserService
    {
        public CurrentUser GetRequiredUser() => currentUser;
    }

    private sealed class FakeProjectRepository : IProjectRepository
    {
        public Task AddAsync(Project project, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Project?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken) => Task.FromResult<Project?>(null);
        public Task<IReadOnlyCollection<Project>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<Project>>(Array.Empty<Project>());
    }

    private sealed class FakeTaskRepository(WorkTask task) : ITaskRepository
    {
        public Task AddAsync(WorkTask taskToAdd, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<WorkTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken) => Task.FromResult<WorkTask?>(task);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public Task AddAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken) => Task.FromResult<ApplicationUser?>(null);
        public Task<ApplicationUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken) => Task.FromResult<ApplicationUser?>(null);
        public Task<ApplicationUser?> GetByIdWithRefreshTokensAsync(Guid userId, CancellationToken cancellationToken) => Task.FromResult<ApplicationUser?>(null);
        public Task<IReadOnlyCollection<ApplicationUser>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<ApplicationUser>>(Array.Empty<ApplicationUser>());
    }

    private sealed class FakeTaskCommentRepository : ITaskCommentRepository
    {
        public Task AddAsync(TaskComment comment, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeActivityLogRepository : IActivityLogRepository
    {
        public Task AddAsync(ActivityLog activityLog, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyCollection<ActivityLog>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<ActivityLog>>(Array.Empty<ActivityLog>());
    }

    private sealed class FakeProjectCache : IProjectCache
    {
        public Task<IReadOnlyCollection<ProjectManagementSaaS.Application.Projects.ProjectResponse>?> GetOrganizationProjectsAsync(Guid organizationId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<ProjectManagementSaaS.Application.Projects.ProjectResponse>?>(null);

        public Task SetOrganizationProjectsAsync(Guid organizationId, IReadOnlyCollection<ProjectManagementSaaS.Application.Projects.ProjectResponse> projects, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task InvalidateOrganizationProjectsAsync(Guid organizationId, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => new(2026, 3, 6, 0, 0, 0, DateTimeKind.Utc);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
    }
}

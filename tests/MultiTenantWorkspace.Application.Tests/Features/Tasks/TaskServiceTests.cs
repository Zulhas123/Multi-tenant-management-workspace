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

    [Fact]
    public async Task UpdateTaskAsync_ShouldUpdateTaskForManager()
    {
        var organizationId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var assigneeId = Guid.NewGuid();
        var task = new WorkTask
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            ProjectId = projectId,
            Title = "Initial task"
        };

        var projectRepository = new FakeProjectRepository(new Project
        {
            Id = projectId,
            OrganizationId = organizationId,
            Name = "Delivery"
        });

        var service = new TaskService(
            new FakeCurrentUserService(new CurrentUser(Guid.NewGuid(), organizationId, "manager@example.com", UserRole.Manager)),
            projectRepository,
            new FakeTaskRepository(task),
            new FakeUserRepository(new ApplicationUser
            {
                Id = assigneeId,
                OrganizationId = organizationId,
                FullName = "Assigned Member"
            }),
            new FakeTaskCommentRepository(),
            new FakeActivityLogRepository(),
            new FakeProjectCache(),
            new FakeDateTimeProvider(),
            new FakeUnitOfWork());

        var result = await service.UpdateTaskAsync(
            task.Id,
            new UpdateTaskRequest(projectId, "Updated task", "Updated description", assigneeId, TaskPriority.High, DomainTaskStatus.InProgress, null),
            CancellationToken.None);

        Assert.Equal("Updated task", result.Title);
        Assert.Equal(TaskPriority.High, result.Priority);
        Assert.Equal(DomainTaskStatus.InProgress, result.Status);
        Assert.Equal(assigneeId, result.AssignedToUserId);
    }

    [Fact]
    public async Task DeleteTaskAsync_ShouldRemoveTask()
    {
        var organizationId = Guid.NewGuid();
        var task = new WorkTask
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            ProjectId = Guid.NewGuid(),
            Title = "Task to delete"
        };

        var repository = new FakeTaskRepository(task);
        var service = new TaskService(
            new FakeCurrentUserService(new CurrentUser(Guid.NewGuid(), organizationId, "admin@example.com", UserRole.Admin)),
            new FakeProjectRepository(new Project
            {
                Id = task.ProjectId,
                OrganizationId = organizationId,
                Name = "Delivery"
            }),
            repository,
            new FakeUserRepository(),
            new FakeTaskCommentRepository(),
            new FakeActivityLogRepository(),
            new FakeProjectCache(),
            new FakeDateTimeProvider(),
            new FakeUnitOfWork());

        await service.DeleteTaskAsync(task.Id, CancellationToken.None);

        Assert.True(repository.Removed);
    }

    private sealed class FakeCurrentUserService(CurrentUser currentUser) : ICurrentUserService
    {
        public CurrentUser GetRequiredUser() => currentUser;
    }

    private sealed class FakeProjectRepository(Project? project = null) : IProjectRepository
    {
        public Task AddAsync(Project project, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<Project?> GetByIdAsync(Guid projectId, CancellationToken cancellationToken) =>
            Task.FromResult<Project?>(project is not null && project.Id == projectId ? project : null);
        public Task<IReadOnlyCollection<Project>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<Project>>(project is null ? Array.Empty<Project>() : new[] { project });
        public void Remove(Project projectToRemove) { }
    }

    private sealed class FakeTaskRepository(WorkTask task) : ITaskRepository
    {
        public bool Removed { get; private set; }

        public Task AddAsync(WorkTask taskToAdd, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<WorkTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken) => Task.FromResult<WorkTask?>(task);

        public void Remove(WorkTask taskToRemove)
        {
            if (taskToRemove.Id == task.Id)
            {
                Removed = true;
            }
        }
    }

    private sealed class FakeUserRepository(ApplicationUser? user = null) : IUserRepository
    {
        public Task AddAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken) => Task.FromResult<ApplicationUser?>(null);
        public Task<ApplicationUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken) =>
            Task.FromResult<ApplicationUser?>(user is not null && user.Id == userId ? user : null);
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

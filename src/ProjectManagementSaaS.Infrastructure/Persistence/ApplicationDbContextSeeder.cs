using ProjectManagementSaaS.Application.Common.Interfaces;
using ProjectManagementSaaS.Domain.Entities;
using ProjectManagementSaaS.Domain.Enums;
using DomainTaskStatus = ProjectManagementSaaS.Domain.Enums.TaskStatus;

namespace ProjectManagementSaaS.Infrastructure.Persistence;

public static class ApplicationDbContextSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext, IPasswordHasher passwordHasher, CancellationToken cancellationToken = default)
    {
        if (dbContext.Organizations.Any())
        {
            return;
        }

        var seedTime = new DateTime(2026, 3, 6, 8, 0, 0, DateTimeKind.Utc);
        var organizationId = Guid.Parse("10000000-0000-0000-0000-000000000001");

        var adminUserId = Guid.Parse("20000000-0000-0000-0000-000000000001");
        var managerUserId = Guid.Parse("20000000-0000-0000-0000-000000000002");
        var memberUserId = Guid.Parse("20000000-0000-0000-0000-000000000003");

        var projectOneId = Guid.Parse("30000000-0000-0000-0000-000000000001");
        var projectTwoId = Guid.Parse("30000000-0000-0000-0000-000000000002");

        var taskOneId = Guid.Parse("40000000-0000-0000-0000-000000000001");
        var taskTwoId = Guid.Parse("40000000-0000-0000-0000-000000000002");
        var taskThreeId = Guid.Parse("40000000-0000-0000-0000-000000000003");

        var commentOneId = Guid.Parse("50000000-0000-0000-0000-000000000001");
        var commentTwoId = Guid.Parse("50000000-0000-0000-0000-000000000002");

        var organization = new Organization
        {
            Id = organizationId,
            Name = "Contoso Delivery Hub",
            Slug = "contoso-delivery",
            CreatedUtc = seedTime
        };

        var adminUser = new ApplicationUser
        {
            Id = adminUserId,
            OrganizationId = organizationId,
            FullName = "Ava Thompson",
            Email = "admin@contoso-demo.com",
            PasswordHash = passwordHasher.Hash("Password123!"),
            Role = UserRole.Admin,
            CreatedUtc = seedTime
        };

        var managerUser = new ApplicationUser
        {
            Id = managerUserId,
            OrganizationId = organizationId,
            FullName = "Noah Bennett",
            Email = "manager@contoso-demo.com",
            PasswordHash = passwordHasher.Hash("Password123!"),
            Role = UserRole.Manager,
            CreatedUtc = seedTime.AddMinutes(5)
        };

        var memberUser = new ApplicationUser
        {
            Id = memberUserId,
            OrganizationId = organizationId,
            FullName = "Mia Chen",
            Email = "member@contoso-demo.com",
            PasswordHash = passwordHasher.Hash("Password123!"),
            Role = UserRole.Member,
            CreatedUtc = seedTime.AddMinutes(10)
        };

        var onboardingProject = new Project
        {
            Id = projectOneId,
            OrganizationId = organizationId,
            CreatedByUserId = adminUserId,
            Name = "Customer Onboarding",
            Description = "Standardize the first 30-day onboarding delivery process for new enterprise customers.",
            Status = ProjectStatus.Active,
            StartDateUtc = seedTime.Date,
            DueDateUtc = seedTime.Date.AddDays(30),
            CreatedUtc = seedTime.AddHours(1),
            CreatedById = adminUserId
        };

        var accessProject = new Project
        {
            Id = projectTwoId,
            OrganizationId = organizationId,
            CreatedByUserId = managerUserId,
            Name = "Workspace Permissions Rollout",
            Description = "Roll out role-based workspace permissions and internal operating guidelines.",
            Status = ProjectStatus.Planned,
            StartDateUtc = seedTime.Date.AddDays(2),
            DueDateUtc = seedTime.Date.AddDays(20),
            CreatedUtc = seedTime.AddHours(2),
            CreatedById = managerUserId
        };

        var taskOne = new WorkTask
        {
            Id = taskOneId,
            OrganizationId = organizationId,
            ProjectId = projectOneId,
            AssignedToUserId = memberUserId,
            Title = "Prepare onboarding checklist",
            Description = "Draft the checklist used during the first customer kickoff call.",
            Status = DomainTaskStatus.InProgress,
            Priority = TaskPriority.High,
            DueDateUtc = seedTime.Date.AddDays(7),
            CreatedUtc = seedTime.AddHours(3),
            CreatedById = managerUserId
        };

        var taskTwo = new WorkTask
        {
            Id = taskTwoId,
            OrganizationId = organizationId,
            ProjectId = projectOneId,
            AssignedToUserId = managerUserId,
            Title = "Define success milestones",
            Description = "Agree target milestones for kickoff, technical setup, and handoff.",
            Status = DomainTaskStatus.Todo,
            Priority = TaskPriority.Medium,
            DueDateUtc = seedTime.Date.AddDays(10),
            CreatedUtc = seedTime.AddHours(3).AddMinutes(15),
            CreatedById = adminUserId
        };

        var taskThree = new WorkTask
        {
            Id = taskThreeId,
            OrganizationId = organizationId,
            ProjectId = projectTwoId,
            AssignedToUserId = memberUserId,
            Title = "Review manager permissions",
            Description = "Verify managers can create projects and assign tasks without admin escalation.",
            Status = DomainTaskStatus.Blocked,
            Priority = TaskPriority.Critical,
            DueDateUtc = seedTime.Date.AddDays(12),
            CreatedUtc = seedTime.AddHours(4),
            CreatedById = managerUserId
        };

        var commentOne = new TaskComment
        {
            Id = commentOneId,
            TaskId = taskOneId,
            AuthorUserId = managerUserId,
            Content = "Start with the kickoff checklist and include ownership per stage.",
            CreatedUtc = seedTime.AddHours(5)
        };

        var commentTwo = new TaskComment
        {
            Id = commentTwoId,
            TaskId = taskThreeId,
            AuthorUserId = memberUserId,
            Content = "Blocked until final role matrix is approved by the admin team.",
            CreatedUtc = seedTime.AddHours(6)
        };

        var activityLogs = new[]
        {
            CreateActivityLog(Guid.Parse("60000000-0000-0000-0000-000000000001"), organizationId, adminUserId, nameof(Organization), organizationId.ToString(), "SeededOrganization", "Demo organization created for first-run experience.", seedTime),
            CreateActivityLog(Guid.Parse("60000000-0000-0000-0000-000000000002"), organizationId, adminUserId, nameof(Project), projectOneId.ToString(), "ProjectCreated", "Project 'Customer Onboarding' seeded.", seedTime.AddHours(1)),
            CreateActivityLog(Guid.Parse("60000000-0000-0000-0000-000000000003"), organizationId, managerUserId, nameof(Project), projectTwoId.ToString(), "ProjectCreated", "Project 'Workspace Permissions Rollout' seeded.", seedTime.AddHours(2)),
            CreateActivityLog(Guid.Parse("60000000-0000-0000-0000-000000000004"), organizationId, managerUserId, nameof(WorkTask), taskOneId.ToString(), "TaskCreated", "Task 'Prepare onboarding checklist' seeded.", seedTime.AddHours(3)),
            CreateActivityLog(Guid.Parse("60000000-0000-0000-0000-000000000005"), organizationId, memberUserId, nameof(TaskComment), commentTwoId.ToString(), "TaskCommentAdded", "Demo blocked-task comment added.", seedTime.AddHours(6))
        };

        await dbContext.Organizations.AddAsync(organization, cancellationToken);
        await dbContext.Users.AddRangeAsync([adminUser, managerUser, memberUser], cancellationToken);
        await dbContext.Projects.AddRangeAsync([onboardingProject, accessProject], cancellationToken);
        await dbContext.Tasks.AddRangeAsync([taskOne, taskTwo, taskThree], cancellationToken);
        await dbContext.TaskComments.AddRangeAsync([commentOne, commentTwo], cancellationToken);
        await dbContext.ActivityLogs.AddRangeAsync(activityLogs, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static ActivityLog CreateActivityLog(
        Guid id,
        Guid organizationId,
        Guid? actorUserId,
        string entityType,
        string entityId,
        string action,
        string description,
        DateTime createdUtc)
    {
        return new ActivityLog
        {
            Id = id,
            OrganizationId = organizationId,
            ActorUserId = actorUserId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Description = description,
            CreatedUtc = createdUtc
        };
    }
}

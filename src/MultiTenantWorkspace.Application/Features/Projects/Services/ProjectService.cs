using ProjectManagementSaaS.Application.Common.Exceptions;
using ProjectManagementSaaS.Application.Common.Interfaces;
using ProjectManagementSaaS.Domain.Entities;

namespace ProjectManagementSaaS.Application.Projects;

public sealed class ProjectService(
    ICurrentUserService currentUserService,
    IProjectRepository projectRepository,
    IActivityLogRepository activityLogRepository,
    IProjectCache projectCache,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : IProjectService
{
    public async Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserService.GetRequiredUser();
        if (!currentUser.IsAdminOrManager)
        {
            throw new ForbiddenException("Only admins and managers can create projects.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Project name is required.");
        }

        var project = new Project
        {
            OrganizationId = currentUser.OrganizationId,
            CreatedByUserId = currentUser.UserId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Status = request.Status,
            StartDateUtc = request.StartDateUtc,
            DueDateUtc = request.DueDateUtc,
            CreatedUtc = dateTimeProvider.UtcNow
        };

        await projectRepository.AddAsync(project, cancellationToken);
        await activityLogRepository.AddAsync(new ActivityLog
        {
            OrganizationId = currentUser.OrganizationId,
            ActorUserId = currentUser.UserId,
            EntityType = nameof(Project),
            EntityId = project.Id.ToString(),
            Action = "ProjectCreated",
            Description = $"Project '{project.Name}' created.",
            CreatedUtc = dateTimeProvider.UtcNow
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await projectCache.InvalidateOrganizationProjectsAsync(currentUser.OrganizationId, cancellationToken);

        return Map(project);
    }

    public async Task<IReadOnlyCollection<ProjectResponse>> GetProjectsAsync(CancellationToken cancellationToken)
    {
        var currentUser = currentUserService.GetRequiredUser();
        var cached = await projectCache.GetOrganizationProjectsAsync(currentUser.OrganizationId, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var projects = await projectRepository.ListByOrganizationAsync(currentUser.OrganizationId, cancellationToken);
        var result = projects.Select(Map).ToArray();
        await projectCache.SetOrganizationProjectsAsync(currentUser.OrganizationId, result, cancellationToken);
        return result;
    }

    public async Task<ProjectResponse> UpdateProjectAsync(Guid projectId, UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserService.GetRequiredUser();
        if (!currentUser.IsAdminOrManager)
        {
            throw new ForbiddenException("Only admins and managers can update projects.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Project name is required.");
        }

        var project = await projectRepository.GetByIdAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Project was not found.");

        if (project.OrganizationId != currentUser.OrganizationId)
        {
            throw new ForbiddenException("Project does not belong to the current organization.");
        }

        project.Name = request.Name.Trim();
        project.Description = request.Description?.Trim();
        project.Status = request.Status;
        project.StartDateUtc = request.StartDateUtc;
        project.DueDateUtc = request.DueDateUtc;
        project.UpdatedUtc = dateTimeProvider.UtcNow;
        project.UpdatedById = currentUser.UserId;

        await activityLogRepository.AddAsync(new ActivityLog
        {
            OrganizationId = currentUser.OrganizationId,
            ActorUserId = currentUser.UserId,
            EntityType = nameof(Project),
            EntityId = project.Id.ToString(),
            Action = "ProjectUpdated",
            Description = $"Project '{project.Name}' updated.",
            CreatedUtc = dateTimeProvider.UtcNow
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await projectCache.InvalidateOrganizationProjectsAsync(currentUser.OrganizationId, cancellationToken);

        return Map(project);
    }

    public async Task DeleteProjectAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var currentUser = currentUserService.GetRequiredUser();
        if (!currentUser.IsAdminOrManager)
        {
            throw new ForbiddenException("Only admins and managers can delete projects.");
        }

        var project = await projectRepository.GetByIdAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Project was not found.");

        if (project.OrganizationId != currentUser.OrganizationId)
        {
            throw new ForbiddenException("Project does not belong to the current organization.");
        }

        await activityLogRepository.AddAsync(new ActivityLog
        {
            OrganizationId = currentUser.OrganizationId,
            ActorUserId = currentUser.UserId,
            EntityType = nameof(Project),
            EntityId = project.Id.ToString(),
            Action = "ProjectDeleted",
            Description = $"Project '{project.Name}' deleted.",
            CreatedUtc = dateTimeProvider.UtcNow
        }, cancellationToken);

        projectRepository.Remove(project);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await projectCache.InvalidateOrganizationProjectsAsync(currentUser.OrganizationId, cancellationToken);
    }

    private static ProjectResponse Map(Project project)
    {
        return new ProjectResponse(
            project.Id,
            project.Name,
            project.Description,
            project.Status,
            project.StartDateUtc,
            project.DueDateUtc,
            project.Tasks
                .OrderBy(x => x.DueDateUtc)
                .Select(x => new ProjectTaskSummaryResponse(
                    x.Id,
                    x.Title,
                    x.Status,
                    x.AssignedToUser?.FullName,
                    x.DueDateUtc))
                .ToArray());
    }
}

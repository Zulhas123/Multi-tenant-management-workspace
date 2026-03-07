using ProjectManagementSaaS.Application.Common.Exceptions;
using ProjectManagementSaaS.Application.Common.Interfaces;
using ProjectManagementSaaS.Domain.Entities;

namespace ProjectManagementSaaS.Application.Tasks;

public sealed class TaskService(
    ICurrentUserService currentUserService,
    IProjectRepository projectRepository,
    ITaskRepository taskRepository,
    IUserRepository userRepository,
    ITaskCommentRepository taskCommentRepository,
    IActivityLogRepository activityLogRepository,
    IProjectCache projectCache,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : ITaskService
{
    public async Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserService.GetRequiredUser();
        if (!currentUser.IsAdminOrManager)
        {
            throw new ForbiddenException("Only admins and managers can create tasks.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ValidationException("Task title is required.");
        }

        var project = await projectRepository.GetByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Project was not found.");

        if (project.OrganizationId != currentUser.OrganizationId)
        {
            throw new ForbiddenException("Project does not belong to the current organization.");
        }

        ApplicationUser? assignee = null;
        if (request.AssignedToUserId.HasValue)
        {
            assignee = await userRepository.GetByIdAsync(request.AssignedToUserId.Value, cancellationToken)
                ?? throw new NotFoundException("Assigned user was not found.");

            if (assignee.OrganizationId != currentUser.OrganizationId)
            {
                throw new ForbiddenException("Assigned user does not belong to the current organization.");
            }
        }

        var task = new WorkTask
        {
            OrganizationId = currentUser.OrganizationId,
            ProjectId = project.Id,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            AssignedToUserId = assignee?.Id,
            Priority = request.Priority,
            DueDateUtc = request.DueDateUtc,
            CreatedUtc = dateTimeProvider.UtcNow
        };

        await taskRepository.AddAsync(task, cancellationToken);
        await activityLogRepository.AddAsync(new ActivityLog
        {
            OrganizationId = currentUser.OrganizationId,
            ActorUserId = currentUser.UserId,
            EntityType = nameof(WorkTask),
            EntityId = task.Id.ToString(),
            Action = "TaskCreated",
            Description = $"Task '{task.Title}' created in project '{project.Name}'.",
            CreatedUtc = dateTimeProvider.UtcNow
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await projectCache.InvalidateOrganizationProjectsAsync(currentUser.OrganizationId, cancellationToken);

        task.AssignedToUser = assignee;
        return Map(task);
    }

    public async Task<TaskResponse> GetTaskAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var currentUser = currentUserService.GetRequiredUser();
        var task = await taskRepository.GetByIdAsync(taskId, cancellationToken)
            ?? throw new NotFoundException("Task was not found.");

        if (task.OrganizationId != currentUser.OrganizationId)
        {
            throw new ForbiddenException("Task does not belong to the current organization.");
        }

        return Map(task);
    }

    public async Task<TaskResponse> UpdateTaskAsync(Guid taskId, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserService.GetRequiredUser();
        if (!currentUser.IsAdminOrManager)
        {
            throw new ForbiddenException("Only admins and managers can update tasks.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ValidationException("Task title is required.");
        }

        var task = await taskRepository.GetByIdAsync(taskId, cancellationToken)
            ?? throw new NotFoundException("Task was not found.");

        if (task.OrganizationId != currentUser.OrganizationId)
        {
            throw new ForbiddenException("Task does not belong to the current organization.");
        }

        var project = await projectRepository.GetByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new NotFoundException("Project was not found.");

        if (project.OrganizationId != currentUser.OrganizationId)
        {
            throw new ForbiddenException("Project does not belong to the current organization.");
        }

        ApplicationUser? assignee = null;
        if (request.AssignedToUserId.HasValue)
        {
            assignee = await userRepository.GetByIdAsync(request.AssignedToUserId.Value, cancellationToken)
                ?? throw new NotFoundException("Assigned user was not found.");

            if (assignee.OrganizationId != currentUser.OrganizationId)
            {
                throw new ForbiddenException("Assigned user does not belong to the current organization.");
            }
        }

        task.ProjectId = request.ProjectId;
        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.AssignedToUserId = assignee?.Id;
        task.AssignedToUser = assignee;
        task.Priority = request.Priority;
        task.Status = request.Status;
        task.DueDateUtc = request.DueDateUtc;
        task.UpdatedUtc = dateTimeProvider.UtcNow;
        task.UpdatedById = currentUser.UserId;

        await activityLogRepository.AddAsync(new ActivityLog
        {
            OrganizationId = currentUser.OrganizationId,
            ActorUserId = currentUser.UserId,
            EntityType = nameof(WorkTask),
            EntityId = task.Id.ToString(),
            Action = "TaskUpdated",
            Description = $"Task '{task.Title}' updated in project '{project.Name}'.",
            CreatedUtc = dateTimeProvider.UtcNow
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await projectCache.InvalidateOrganizationProjectsAsync(currentUser.OrganizationId, cancellationToken);

        return Map(task);
    }

    public async Task<TaskResponse> UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusRequest request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserService.GetRequiredUser();
        var task = await taskRepository.GetByIdAsync(taskId, cancellationToken)
            ?? throw new NotFoundException("Task was not found.");

        if (task.OrganizationId != currentUser.OrganizationId)
        {
            throw new ForbiddenException("Task does not belong to the current organization.");
        }

        var isAssignedUser = task.AssignedToUserId == currentUser.UserId;
        if (!currentUser.IsAdminOrManager && !isAssignedUser)
        {
            throw new ForbiddenException("Only managers, admins, or the assigned member can update task status.");
        }

        task.Status = request.Status;
        task.UpdatedUtc = dateTimeProvider.UtcNow;
        task.UpdatedById = currentUser.UserId;

        await activityLogRepository.AddAsync(new ActivityLog
        {
            OrganizationId = currentUser.OrganizationId,
            ActorUserId = currentUser.UserId,
            EntityType = nameof(WorkTask),
            EntityId = task.Id.ToString(),
            Action = "TaskStatusUpdated",
            Description = $"Task '{task.Title}' status updated to '{request.Status}'.",
            CreatedUtc = dateTimeProvider.UtcNow
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await projectCache.InvalidateOrganizationProjectsAsync(currentUser.OrganizationId, cancellationToken);

        return Map(task);
    }

    public async Task<TaskCommentResponse> AddCommentAsync(Guid taskId, AddTaskCommentRequest request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserService.GetRequiredUser();
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ValidationException("Comment content is required.");
        }

        var task = await taskRepository.GetByIdAsync(taskId, cancellationToken)
            ?? throw new NotFoundException("Task was not found.");

        if (task.OrganizationId != currentUser.OrganizationId)
        {
            throw new ForbiddenException("Task does not belong to the current organization.");
        }

        var comment = new TaskComment
        {
            TaskId = task.Id,
            AuthorUserId = currentUser.UserId,
            Content = request.Content.Trim(),
            CreatedUtc = dateTimeProvider.UtcNow
        };

        await taskCommentRepository.AddAsync(comment, cancellationToken);
        await activityLogRepository.AddAsync(new ActivityLog
        {
            OrganizationId = currentUser.OrganizationId,
            ActorUserId = currentUser.UserId,
            EntityType = nameof(TaskComment),
            EntityId = comment.Id.ToString(),
            Action = "TaskCommentAdded",
            Description = $"Comment added to task '{task.Title}'.",
            CreatedUtc = dateTimeProvider.UtcNow
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new TaskCommentResponse(comment.Id, comment.AuthorUserId, comment.Content, comment.CreatedUtc);
    }

    public async Task DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var currentUser = currentUserService.GetRequiredUser();
        if (!currentUser.IsAdminOrManager)
        {
            throw new ForbiddenException("Only admins and managers can delete tasks.");
        }

        var task = await taskRepository.GetByIdAsync(taskId, cancellationToken)
            ?? throw new NotFoundException("Task was not found.");

        if (task.OrganizationId != currentUser.OrganizationId)
        {
            throw new ForbiddenException("Task does not belong to the current organization.");
        }

        await activityLogRepository.AddAsync(new ActivityLog
        {
            OrganizationId = currentUser.OrganizationId,
            ActorUserId = currentUser.UserId,
            EntityType = nameof(WorkTask),
            EntityId = task.Id.ToString(),
            Action = "TaskDeleted",
            Description = $"Task '{task.Title}' deleted.",
            CreatedUtc = dateTimeProvider.UtcNow
        }, cancellationToken);

        taskRepository.Remove(task);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await projectCache.InvalidateOrganizationProjectsAsync(currentUser.OrganizationId, cancellationToken);
    }

    private static TaskResponse Map(WorkTask task)
    {
        return new TaskResponse(
            task.Id,
            task.ProjectId,
            task.Title,
            task.Description,
            task.AssignedToUserId,
            task.AssignedToUser?.FullName,
            task.Status,
            task.Priority,
            task.DueDateUtc,
            task.Comments
                .OrderBy(x => x.CreatedUtc)
                .Select(x => new TaskCommentResponse(x.Id, x.AuthorUserId, x.Content, x.CreatedUtc))
                .ToArray());
    }
}

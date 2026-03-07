using ProjectManagementSaaS.Application.Common.Interfaces;
using ProjectManagementSaaS.Domain.Entities;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Repositories;

public sealed class TaskCommentRepository(ApplicationDbContext dbContext) : ITaskCommentRepository
{
    public Task AddAsync(TaskComment comment, CancellationToken cancellationToken) =>
        dbContext.TaskComments.AddAsync(comment, cancellationToken).AsTask();
}

using Microsoft.EntityFrameworkCore;
using ProjectManagementSaaS.Application.Common.Interfaces;
using ProjectManagementSaaS.Domain.Entities;

namespace ProjectManagementSaaS.Infrastructure.Persistence.Repositories;

public sealed class TaskRepository(ApplicationDbContext dbContext) : ITaskRepository
{
    public Task AddAsync(WorkTask task, CancellationToken cancellationToken) =>
        dbContext.Tasks.AddAsync(task, cancellationToken).AsTask();

    public Task<WorkTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken) =>
        dbContext.Tasks
            .Include(x => x.AssignedToUser)
            .Include(x => x.Comments)
            .FirstOrDefaultAsync(x => x.Id == taskId, cancellationToken);
}

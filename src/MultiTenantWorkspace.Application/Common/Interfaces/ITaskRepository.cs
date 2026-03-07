using ProjectManagementSaaS.Domain.Entities;

namespace ProjectManagementSaaS.Application.Common.Interfaces;

public interface ITaskRepository
{
    Task AddAsync(WorkTask task, CancellationToken cancellationToken);
    Task<WorkTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken);
    void Remove(WorkTask task);
}

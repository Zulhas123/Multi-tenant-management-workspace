using ProjectManagementSaaS.Domain.Entities;

namespace ProjectManagementSaaS.Application.Common.Interfaces;

public interface ITaskCommentRepository
{
    Task AddAsync(TaskComment comment, CancellationToken cancellationToken);
}

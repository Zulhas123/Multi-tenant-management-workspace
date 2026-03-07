namespace ProjectManagementSaaS.Application.Tasks;

public interface ITaskService
{
    Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken);
    Task<TaskResponse> GetTaskAsync(Guid taskId, CancellationToken cancellationToken);
    Task<TaskResponse> UpdateTaskAsync(Guid taskId, UpdateTaskRequest request, CancellationToken cancellationToken);
    Task<TaskResponse> UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusRequest request, CancellationToken cancellationToken);
    Task<TaskCommentResponse> AddCommentAsync(Guid taskId, AddTaskCommentRequest request, CancellationToken cancellationToken);
    Task DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken);
}

namespace ProjectManagementSaaS.Application.Projects;

public interface IProjectService
{
    Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ProjectResponse>> GetProjectsAsync(CancellationToken cancellationToken);
    Task<ProjectResponse> UpdateProjectAsync(Guid projectId, UpdateProjectRequest request, CancellationToken cancellationToken);
    Task DeleteProjectAsync(Guid projectId, CancellationToken cancellationToken);
}

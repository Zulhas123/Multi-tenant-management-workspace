namespace ProjectManagementSaaS.Application.Projects;

public interface IProjectService
{
    Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ProjectResponse>> GetProjectsAsync(CancellationToken cancellationToken);
}

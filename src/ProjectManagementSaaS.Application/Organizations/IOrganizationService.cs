namespace ProjectManagementSaaS.Application.Organizations;

public interface IOrganizationService
{
    Task<OrganizationSummaryResponse> GetCurrentOrganizationAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<OrganizationUserResponse>> GetOrganizationUsersAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ActivityLogResponse>> GetActivityLogsAsync(CancellationToken cancellationToken);
}

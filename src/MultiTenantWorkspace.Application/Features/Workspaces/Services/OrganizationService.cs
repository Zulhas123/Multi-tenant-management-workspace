using ProjectManagementSaaS.Application.Common.Exceptions;
using ProjectManagementSaaS.Application.Common.Interfaces;

namespace ProjectManagementSaaS.Application.Organizations;

public sealed class OrganizationService(
    ICurrentUserService currentUserService,
    IOrganizationRepository organizationRepository,
    IUserRepository userRepository,
    IActivityLogRepository activityLogRepository) : IOrganizationService
{
    public async Task<OrganizationSummaryResponse> GetCurrentOrganizationAsync(CancellationToken cancellationToken)
    {
        var currentUser = currentUserService.GetRequiredUser();
        var organization = await organizationRepository.GetByIdAsync(currentUser.OrganizationId, cancellationToken)
            ?? throw new NotFoundException("Organization was not found.");

        return new OrganizationSummaryResponse(organization.Id, organization.Name, organization.Slug);
    }

    public async Task<IReadOnlyCollection<OrganizationUserResponse>> GetOrganizationUsersAsync(CancellationToken cancellationToken)
    {
        var currentUser = currentUserService.GetRequiredUser();
        var users = await userRepository.ListByOrganizationAsync(currentUser.OrganizationId, cancellationToken);

        return users
            .OrderBy(x => x.FullName)
            .Select(x => new OrganizationUserResponse(x.Id, x.FullName, x.Email, x.Role))
            .ToArray();
    }

    public async Task<IReadOnlyCollection<ActivityLogResponse>> GetActivityLogsAsync(CancellationToken cancellationToken)
    {
        var currentUser = currentUserService.GetRequiredUser();
        var logs = await activityLogRepository.ListByOrganizationAsync(currentUser.OrganizationId, cancellationToken);

        return logs
            .OrderByDescending(x => x.CreatedUtc)
            .Select(x => new ActivityLogResponse(
                x.Id,
                x.EntityType,
                x.EntityId,
                x.Action,
                x.Description,
                x.ActorUserId,
                x.CreatedUtc))
            .ToArray();
    }
}

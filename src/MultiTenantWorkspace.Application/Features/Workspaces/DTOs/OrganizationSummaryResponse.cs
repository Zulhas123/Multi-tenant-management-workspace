namespace ProjectManagementSaaS.Application.Organizations;

public sealed record OrganizationSummaryResponse(Guid OrganizationId, string Name, string Slug);

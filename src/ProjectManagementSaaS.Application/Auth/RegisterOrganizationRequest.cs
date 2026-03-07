namespace ProjectManagementSaaS.Application.Auth;

public sealed record RegisterOrganizationRequest(
    string OrganizationName,
    string OrganizationSlug,
    string AdminFullName,
    string AdminEmail,
    string Password);

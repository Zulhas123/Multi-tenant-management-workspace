using Microsoft.Extensions.DependencyInjection;
using ProjectManagementSaaS.Application.Auth;
using ProjectManagementSaaS.Application.Organizations;
using ProjectManagementSaaS.Application.Projects;
using ProjectManagementSaaS.Application.Tasks;

namespace ProjectManagementSaaS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITaskService, TaskService>();
        return services;
    }
}

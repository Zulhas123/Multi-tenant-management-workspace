using ProjectManagementSaaS.Application.Common.Security;

namespace ProjectManagementSaaS.Application.Common.Interfaces;

public interface ICurrentUserService
{
    CurrentUser GetRequiredUser();
}

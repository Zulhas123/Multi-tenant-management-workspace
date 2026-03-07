using ProjectManagementSaaS.Application.Common.Interfaces;

namespace ProjectManagementSaaS.Infrastructure.Services;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}

namespace ProjectManagementSaaS.Infrastructure.Security;

public sealed class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";
    public int ExpiryDays { get; set; } = 7;
}

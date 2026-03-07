namespace ProjectManagementSaaS.Application.Common.Interfaces;

public interface IRefreshTokenGenerator
{
    string GenerateToken();
    string HashToken(string token);
    DateTime GetExpiryUtc(DateTime utcNow);
}

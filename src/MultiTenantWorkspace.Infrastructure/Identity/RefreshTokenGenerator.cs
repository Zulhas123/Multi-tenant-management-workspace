using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using ProjectManagementSaaS.Application.Common.Interfaces;

namespace ProjectManagementSaaS.Infrastructure.Security;

public sealed class RefreshTokenGenerator(IOptions<RefreshTokenOptions> options) : IRefreshTokenGenerator
{
    private readonly RefreshTokenOptions _options = options.Value;

    public string GenerateToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public string HashToken(string token)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes);
    }

    public DateTime GetExpiryUtc(DateTime utcNow) => utcNow.AddDays(_options.ExpiryDays);
}

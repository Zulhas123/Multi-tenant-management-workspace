using ProjectManagementSaaS.Application.Auth;
using ProjectManagementSaaS.Application.Common.Exceptions;
using ProjectManagementSaaS.Application.Common.Interfaces;
using ProjectManagementSaaS.Domain.Entities;
using ProjectManagementSaaS.Domain.Enums;

namespace ProjectManagementSaaS.Application.Tests;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task RegisterOrganizationAsync_ShouldCreateAdminUser_WhenInputIsValid()
    {
        var organizationRepository = new FakeOrganizationRepository();
        var userRepository = new FakeUserRepository();
        var service = new AuthService(
            organizationRepository,
            userRepository,
            new FakePasswordHasher(),
            new FakeJwtTokenGenerator(),
            new FakeRefreshTokenRepository(),
            new FakeRefreshTokenGenerator(),
            new FakeActivityLogRepository(),
            new FakeDateTimeProvider(),
            new FakeUnitOfWork());

        var response = await service.RegisterOrganizationAsync(
            new RegisterOrganizationRequest("Acme", "acme", "Ada Lovelace", "ada@acme.com", "Password123!"),
            CancellationToken.None);

        Assert.Equal(UserRole.Admin, response.Role);
        Assert.Equal("ada@acme.com", response.Email);
        Assert.False(string.IsNullOrWhiteSpace(response.RefreshToken));
        Assert.Single(organizationRepository.StoredOrganizations);
        Assert.Single(userRepository.StoredUsers);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordDoesNotMatch()
    {
        var userRepository = new FakeUserRepository();
        userRepository.StoredUsers.Add(new ApplicationUser
        {
            FullName = "Grace Hopper",
            Email = "grace@example.com",
            PasswordHash = "hashed-expected",
            OrganizationId = Guid.NewGuid(),
            Role = UserRole.Manager
        });

        var service = new AuthService(
            new FakeOrganizationRepository(),
            userRepository,
            new FakePasswordHasher(),
            new FakeJwtTokenGenerator(),
            new FakeRefreshTokenRepository(),
            new FakeRefreshTokenGenerator(),
            new FakeActivityLogRepository(),
            new FakeDateTimeProvider(),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.LoginAsync(new LoginRequest("grace@example.com", "wrong-password"), CancellationToken.None));
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldRotateToken_WhenRefreshTokenIsValid()
    {
        var userRepository = new FakeUserRepository();
        var refreshTokenRepository = new FakeRefreshTokenRepository();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = "Grace Hopper",
            Email = "grace@example.com",
            PasswordHash = "hashed-Password123!",
            OrganizationId = Guid.NewGuid(),
            Role = UserRole.Manager
        };
        userRepository.StoredUsers.Add(user);

        refreshTokenRepository.StoredTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "hash-seed-token",
            ExpiresUtc = new DateTime(2026, 3, 13, 0, 0, 0, DateTimeKind.Utc)
        });

        var service = new AuthService(
            new FakeOrganizationRepository(),
            userRepository,
            new FakePasswordHasher(),
            new FakeJwtTokenGenerator(),
            refreshTokenRepository,
            new FakeRefreshTokenGenerator(),
            new FakeActivityLogRepository(),
            new FakeDateTimeProvider(),
            new FakeUnitOfWork());

        var response = await service.RefreshTokenAsync(new RefreshTokenRequest("seed-token"), CancellationToken.None);

        Assert.Equal("refresh-1", response.RefreshToken);
        Assert.Contains(refreshTokenRepository.StoredTokens, x => x.TokenHash == "hash-refresh-1");
        Assert.Equal("hash-refresh-1", refreshTokenRepository.StoredTokens[0].ReplacedByTokenHash);
        Assert.NotNull(refreshTokenRepository.StoredTokens[0].RevokedUtc);
    }

    [Fact]
    public async Task LogoutAsync_ShouldRevokeRefreshToken_WhenOwnedByCurrentUser()
    {
        var userId = Guid.NewGuid();
        var userRepository = new FakeUserRepository();
        userRepository.StoredUsers.Add(new ApplicationUser
        {
            Id = userId,
            FullName = "Grace Hopper",
            Email = "grace@example.com",
            PasswordHash = "hashed-Password123!",
            OrganizationId = Guid.NewGuid(),
            Role = UserRole.Manager
        });

        var refreshTokenRepository = new FakeRefreshTokenRepository();
        refreshTokenRepository.StoredTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = "hash-seed-token",
            ExpiresUtc = new DateTime(2026, 3, 13, 0, 0, 0, DateTimeKind.Utc)
        });

        var service = new AuthService(
            new FakeOrganizationRepository(),
            userRepository,
            new FakePasswordHasher(),
            new FakeJwtTokenGenerator(),
            refreshTokenRepository,
            new FakeRefreshTokenGenerator(),
            new FakeActivityLogRepository(),
            new FakeDateTimeProvider(),
            new FakeUnitOfWork());

        await service.LogoutAsync(userId, new LogoutRequest("seed-token"), CancellationToken.None);

        Assert.NotNull(refreshTokenRepository.StoredTokens[0].RevokedUtc);
        Assert.Equal("Logout", refreshTokenRepository.StoredTokens[0].ReasonRevoked);
    }

    private sealed class FakeOrganizationRepository : IOrganizationRepository
    {
        public List<Organization> StoredOrganizations { get; } = [];

        public Task AddAsync(Organization organization, CancellationToken cancellationToken)
        {
            StoredOrganizations.Add(organization);
            return Task.CompletedTask;
        }

        public Task<Organization?> GetByIdAsync(Guid organizationId, CancellationToken cancellationToken) =>
            Task.FromResult(StoredOrganizations.FirstOrDefault(x => x.Id == organizationId));

        public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken) =>
            Task.FromResult(StoredOrganizations.Any(x => x.Slug == slug));
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<ApplicationUser> StoredUsers { get; } = [];

        public Task AddAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            StoredUsers.Add(user);
            return Task.CompletedTask;
        }

        public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) =>
            Task.FromResult(StoredUsers.Any(x => x.Email == email));

        public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
            Task.FromResult(StoredUsers.FirstOrDefault(x => x.Email == email));

        public Task<ApplicationUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken) =>
            Task.FromResult(StoredUsers.FirstOrDefault(x => x.Id == userId));

        public Task<ApplicationUser?> GetByIdWithRefreshTokensAsync(Guid userId, CancellationToken cancellationToken) =>
            Task.FromResult(StoredUsers.FirstOrDefault(x => x.Id == userId));

        public Task<IReadOnlyCollection<ApplicationUser>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<ApplicationUser>>(StoredUsers.Where(x => x.OrganizationId == organizationId).ToArray());
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"hashed-{password}";
        public bool Verify(string password, string passwordHash) => Hash(password) == passwordHash;
    }

    private sealed class FakeJwtTokenGenerator : IJwtTokenGenerator
    {
        public string GenerateToken(ApplicationUser user) => $"token-{user.Email}";
    }

    private sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
    {
        public List<RefreshToken> StoredTokens { get; } = [];

        public Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
        {
            StoredTokens.Add(refreshToken);
            return Task.CompletedTask;
        }

        public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken) =>
            Task.FromResult(StoredTokens.FirstOrDefault(x => x.TokenHash == tokenHash));
    }

    private sealed class FakeRefreshTokenGenerator : IRefreshTokenGenerator
    {
        private int _counter = 1;

        public string GenerateToken() => $"refresh-{_counter++}";
        public string HashToken(string token) => $"hash-{token}";
        public DateTime GetExpiryUtc(DateTime utcNow) => utcNow.AddDays(7);
    }

    private sealed class FakeActivityLogRepository : IActivityLogRepository
    {
        public Task AddAsync(ActivityLog activityLog, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyCollection<ActivityLog>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<ActivityLog>>(Array.Empty<ActivityLog>());
    }

    private sealed class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => new(2026, 3, 6, 0, 0, 0, DateTimeKind.Utc);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
    }
}

using ProjectManagementSaaS.Application.Common.Exceptions;
using ProjectManagementSaaS.Application.Common.Interfaces;
using ProjectManagementSaaS.Domain.Entities;
using ProjectManagementSaaS.Domain.Enums;

namespace ProjectManagementSaaS.Application.Auth;

public sealed class AuthService(
    IOrganizationRepository organizationRepository,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenRepository refreshTokenRepository,
    IRefreshTokenGenerator refreshTokenGenerator,
    IActivityLogRepository activityLogRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : IAuthService
{
    public async Task<AuthResponse> RegisterOrganizationAsync(RegisterOrganizationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.OrganizationName) ||
            string.IsNullOrWhiteSpace(request.OrganizationSlug) ||
            string.IsNullOrWhiteSpace(request.AdminFullName) ||
            string.IsNullOrWhiteSpace(request.AdminEmail) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("All registration fields are required.");
        }

        var slug = request.OrganizationSlug.Trim().ToLowerInvariant();
        var email = request.AdminEmail.Trim().ToLowerInvariant();

        if (await organizationRepository.SlugExistsAsync(slug, cancellationToken))
        {
            throw new ValidationException("Organization slug is already in use.");
        }

        if (await userRepository.EmailExistsAsync(email, cancellationToken))
        {
            throw new ValidationException("Email is already registered.");
        }

        var organization = new Organization
        {
            Name = request.OrganizationName.Trim(),
            Slug = slug,
            CreatedUtc = dateTimeProvider.UtcNow
        };

        var adminUser = new ApplicationUser
        {
            Organization = organization,
            OrganizationId = organization.Id,
            FullName = request.AdminFullName.Trim(),
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = UserRole.Admin,
            CreatedUtc = dateTimeProvider.UtcNow
        };

        await organizationRepository.AddAsync(organization, cancellationToken);
        await userRepository.AddAsync(adminUser, cancellationToken);
        await activityLogRepository.AddAsync(new ActivityLog
        {
            Organization = organization,
            OrganizationId = organization.Id,
            ActorUserId = adminUser.Id,
            EntityType = nameof(Organization),
            EntityId = organization.Id.ToString(),
            Action = "OrganizationRegistered",
            Description = $"Organization '{organization.Name}' registered.",
            CreatedUtc = dateTimeProvider.UtcNow
        }, cancellationToken);

        var response = await CreateAuthResponseAsync(adminUser, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("Email and password are required.");
        }

        var user = await userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken)
            ?? throw new ValidationException("Invalid email or password.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new ValidationException("Invalid email or password.");
        }

        var response = await CreateAuthResponseAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return response;
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new ValidationException("Refresh token is required.");
        }

        var tokenHash = refreshTokenGenerator.HashToken(request.RefreshToken.Trim());
        var existingToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken)
            ?? throw new ValidationException("Refresh token is invalid.");

        var utcNow = dateTimeProvider.UtcNow;
        if (!existingToken.IsActive(utcNow))
        {
            throw new ValidationException("Refresh token is expired or revoked.");
        }

        var user = await userRepository.GetByIdAsync(existingToken.UserId, cancellationToken)
            ?? throw new ValidationException("Refresh token user was not found.");

        existingToken.RevokedUtc = utcNow;
        existingToken.ReasonRevoked = "Rotated";

        var response = await CreateAuthResponseAsync(user, cancellationToken, existingToken);

        await activityLogRepository.AddAsync(new ActivityLog
        {
            OrganizationId = user.OrganizationId,
            ActorUserId = user.Id,
            EntityType = nameof(RefreshToken),
            EntityId = existingToken.Id.ToString(),
            Action = "RefreshTokenRotated",
            Description = $"Refresh token rotated for '{user.Email}'.",
            CreatedUtc = utcNow
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return response;
    }

    public async Task LogoutAsync(Guid currentUserId, LogoutRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new ValidationException("Refresh token is required.");
        }

        var tokenHash = refreshTokenGenerator.HashToken(request.RefreshToken.Trim());
        var existingToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken)
            ?? throw new ValidationException("Refresh token is invalid.");

        if (existingToken.UserId != currentUserId)
        {
            throw new ForbiddenException("Refresh token does not belong to the current user.");
        }

        if (existingToken.RevokedUtc is null)
        {
            existingToken.RevokedUtc = dateTimeProvider.UtcNow;
            existingToken.ReasonRevoked = "Logout";
        }

        var user = await userRepository.GetByIdAsync(currentUserId, cancellationToken)
            ?? throw new NotFoundException("User was not found.");

        await activityLogRepository.AddAsync(new ActivityLog
        {
            OrganizationId = user.OrganizationId,
            ActorUserId = user.Id,
            EntityType = nameof(RefreshToken),
            EntityId = existingToken.Id.ToString(),
            Action = "Logout",
            Description = $"User '{user.Email}' logged out and invalidated the refresh token.",
            CreatedUtc = dateTimeProvider.UtcNow
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(
        ApplicationUser user,
        CancellationToken cancellationToken,
        RefreshToken? replacedToken = null)
    {
        var rawRefreshToken = refreshTokenGenerator.GenerateToken();
        var refreshTokenExpiryUtc = refreshTokenGenerator.GetExpiryUtc(dateTimeProvider.UtcNow);
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenGenerator.HashToken(rawRefreshToken),
            ExpiresUtc = refreshTokenExpiryUtc,
            CreatedUtc = dateTimeProvider.UtcNow
        };

        if (replacedToken is not null)
        {
            replacedToken.ReplacedByTokenHash = refreshToken.TokenHash;
        }

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        return new AuthResponse(
            jwtTokenGenerator.GenerateToken(user),
            rawRefreshToken,
            refreshTokenExpiryUtc,
            user.Id,
            user.OrganizationId,
            user.FullName,
            user.Email,
            user.Role);
    }
}

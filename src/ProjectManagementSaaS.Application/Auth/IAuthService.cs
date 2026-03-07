namespace ProjectManagementSaaS.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterOrganizationAsync(RegisterOrganizationRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken);
    Task LogoutAsync(Guid currentUserId, LogoutRequest request, CancellationToken cancellationToken);
}

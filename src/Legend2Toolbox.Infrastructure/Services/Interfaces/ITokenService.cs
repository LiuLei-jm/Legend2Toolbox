namespace Legend2Toolbox.Infrastructure.Services.Interfaces;

public interface ITokenService
{
    Task<(string AccessToken, DateTimeOffset ExpiresAt)> GenerateAccessTokenAsync(ApplicationUser user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
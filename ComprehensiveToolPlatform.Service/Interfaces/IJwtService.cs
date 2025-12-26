using System.Security.Claims;

namespace ComprehensiveToolPlatform.Service
{
    public interface IJwtService
    {
        string GenerateToken(string userId, string userName);

        ClaimsPrincipal ValidateToken(string token);

        bool TryValidateToken(string token, out ClaimsPrincipal principal);
    }
}

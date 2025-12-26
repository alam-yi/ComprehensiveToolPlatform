using Microsoft.Extensions.Options;

namespace ComprehensiveToolPlatform.Service
{
    public class AuthService : IAuthService
    {
        private readonly IJwtService _jwtService;

        public AuthService(
            IJwtService jwtService)
        {
            _jwtService = jwtService;
        }

        public string LoginAsync(string account, string password)
        {
            // 验证用户账号密码

            // 生成令牌
            var tokenResult = _jwtService.GenerateToken(account, account);

            // 记录登录日志...

            return tokenResult;
        }

        //public string RefreshTokenAsync(string token)
        //{
        //    var principal = _jwtService.ValidateToken(token);
        //    if (principal == null)
        //    {
        //        throw new Exception("无效的访问令牌");
        //    }

        //    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    var user = await _userRepository.GetByIdAsync(long.Parse(userId));

        //    if (user == null || !user.IsActive)
        //    {
        //        return ApiResponse<JwtTokenResult>.Fail("用户不存在或已被禁用");
        //    }

        //    var newToken = _jwtService.GenerateToken(user);
        //    return newToken;
        //}

        //public void LogoutAsync(string token)
        //{
        //    var principal = _jwtService.ValidateToken(token);
        //    if (principal != null)
        //    {
        //        var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        //        var exp = principal.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;

        //        if (!string.IsNullOrEmpty(jti) && long.TryParse(exp, out var expTime))
        //        {
        //            await _blacklistService.AddToBlacklistAsync(jti,
        //                DateTimeOffset.FromUnixTimeSeconds(expTime).DateTime);
        //        }
        //    }
        //}

        
    }
}

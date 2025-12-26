namespace ComprehensiveToolPlatform.Common
{
    public class JwtSettings
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int AccessTokenExpiration { get; set; } // 分钟
        public int RefreshTokenExpiration { get; set; } // 天
    }
}

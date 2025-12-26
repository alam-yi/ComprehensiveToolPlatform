namespace ComprehensiveToolPlatform.Service
{
    public interface IAuthService
    {
        string LoginAsync(string account, string password);

        //string RefreshTokenAsync(RefreshTokenRequest request);

        //void LogoutAsync(string token);
    }
}

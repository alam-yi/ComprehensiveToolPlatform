using ComprehensiveToolPlatform.Service;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ComprehensiveToolPlatform
{
    public class JwtAuthorizationFilterFactory : IFilterFactory
    {
        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            // 从服务容器中解析JWT服务
            var jwtService = serviceProvider.GetRequiredService<IJwtService>();
            return new JwtAuthorizationFilter(jwtService);
        }
    }
}

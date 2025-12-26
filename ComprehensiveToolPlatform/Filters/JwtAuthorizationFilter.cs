using ComprehensiveToolPlatform.Common;
using ComprehensiveToolPlatform.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ComprehensiveToolPlatform
{
    public class JwtAuthorizationFilter : IAuthorizationFilter
    {
        private readonly IJwtService _jwtService;
        public JwtAuthorizationFilter(IJwtService jwtService)
        {
            _jwtService = jwtService;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // 检查是否允许匿名访问 [citation:2]
            if (HasAllowAnonymous(context) || IsLoginPage(context))
            {
                return;
            }

            // 从 Cookie 读取
            var token = context.HttpContext.Request.Cookies["accessToken"];
            //if (string.IsNullOrEmpty(token))
            //    token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                // 未提供Token，跳转到登录页面 [citation:1][citation:7]
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            if (!_jwtService.TryValidateToken(token, out var principal))
            {
                // Token验证失败，跳转到登录页面
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path });
                return;
            }
            // 验证通过，将用户信息存入HttpContext
            context.HttpContext.User = principal;
        }

        private bool HasAllowAnonymous(AuthorizationFilterContext context)
        {
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (actionDescriptor == null)
            {
                return false;
            }

            // 检查Controller级别 [citation:2]
            var controllerType = actionDescriptor.ControllerTypeInfo;
            if (controllerType.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any())
            {
                return true;
            }

            // 检查Action级别
            var methodInfo = actionDescriptor.MethodInfo;
            if (methodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any())
            {
                return true;
            }

            return false;
        }

        private bool IsLoginPage(AuthorizationFilterContext context)
        {
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (actionDescriptor == null)
            {
                return false;
            }

            // 检查是否是登录页面
            return actionDescriptor.ControllerName.Equals("Account", StringComparison.OrdinalIgnoreCase) &&
                   actionDescriptor.ActionName.Equals("Login", StringComparison.OrdinalIgnoreCase);
        }
    }
}

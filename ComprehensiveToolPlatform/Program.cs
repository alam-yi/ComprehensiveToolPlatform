using ComprehensiveToolPlatform;
using ComprehensiveToolPlatform.Common;
using ComprehensiveToolPlatform.Repository.Contexts;
using ComprehensiveToolPlatform.Service;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 3221225472L; // 3GB
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 3221225472L; // 3GB
});

builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 3221225472L; // 3GB
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// 配置服务
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddDbContext<EfCoreContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();

builder.Services.AddControllersWithViews(options =>
{
    // 使用工厂模式注册全局授权过滤器，支持依赖注入
    options.Filters.Add<JwtAuthorizationFilterFactory>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 添加自定义异常处理中间件
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        LogHelper.LocalLog("GlobalException",
            $"Path: {context.Request.Path}, Error: {ex.Message}, Stack: {ex.StackTrace}");
        throw;
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
using ComprehensiveToolPlatform.Common;
using ComprehensiveToolPlatform.Repository.Models;
using ComprehensiveToolPlatform.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ComprehensiveToolPlatform.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly IApplicationService _applicationService;

        public AccountController(
            IAuthService authService, 
            IConfiguration configuration,
            IApplicationService applicationService)
        {
            _authService = authService;
            _configuration = configuration;
            _applicationService = applicationService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.UserName))
            {
                return Json(ApiResponse.Error(-1, "请输入账号"));
            }
            if (string.IsNullOrWhiteSpace(req.Password))
            {
                return Json(ApiResponse.Error(-1, "请输入密码"));
            }

            var token = _authService.LoginAsync(req.UserName, req.Password);

            // 创建 Cookie
            double accessTokenExpiration = double.Parse(_configuration["JwtSettings:AccessTokenExpiration"]);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,          // 开发环境可设为 false
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(accessTokenExpiration)
            };
                
            Response.Cookies.Append("accessToken", token, cookieOptions);

            return Json(ApiResponse.Ok());
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            Response.Cookies.Delete("accessToken");
            return Json(ApiResponse.Ok());
        }

        public IActionResult CategorySetting()
        {
            return View();
        }

        [HttpGet("account/categories/lite")]
        public IActionResult GetCategoriesLite()
        {
            var vos = _applicationService.GetCategories();
            return Json(ApiResponse<List<Category>>.Ok(vos));
        }

        // 更新类别
        [HttpPut("account/category/{id}")]
        public IActionResult UpdateCategory(string id, [FromBody] CategoryUpdateDto dto)
        {
            try
            {
                _applicationService.UpdateCategory(id, dto);
            }
            catch (Exception ex)
            {
                return Json(ApiResponse.Error(-1, ex.Message));
            }
           
            return Json(ApiResponse.Ok());
        }

        // 删除类别
        [HttpDelete("account/category/{id}")]
        public IActionResult DeleteCategory(string id)
        {
            try
            {
                _applicationService.DeleteCategory(id);
            }
            catch (Exception ex)
            {
                return Json(ApiResponse.Error(-1, ex.Message));
            }

            return Json(ApiResponse.Ok());
        }

        // 添加类别
        [HttpPost("account/category")]
        public IActionResult AddCategory([FromBody] CategoryCreateDto dto)
        {
            try
            {
                _applicationService.AddCategory(dto);
            }
            catch (Exception ex)
            {
                return Json(ApiResponse.Error(-1, ex.Message));
            }

            return Json(ApiResponse.Ok());
        }

        [HttpPost("account/categories/update-sort")]
        public IActionResult UpdateCategoriesSort([FromBody] List<SortUpdateDto> sortUpdates)
        {
            try
            {
                _applicationService.UpdateCategoriesSort(sortUpdates);
            }
            catch (Exception ex)
            {
                return Json(ApiResponse.Error(-1, ex.Message));
            }
            return Json(ApiResponse.Ok());
        }

        [HttpGet("account/applications/{categoryId}")]
        public IActionResult GetApplications(string categoryId)
        {
            var vos = _applicationService.GetApplications(categoryId);
            return Json(ApiResponse<List<Application>>.Ok(vos));
        }

        // 更新应用
        [HttpPut("account/application/{id}")]
        public IActionResult UpdateApplication(string id, [FromBody] ApplicationUpdateDto dto)
        {
            try
            {
                _applicationService.UpdateApplication(id, dto);
            }
            catch (Exception ex)
            {
                return Json(ApiResponse.Error(-1, ex.Message));
            }

            return Json(ApiResponse.Ok());
        }

        // 删除应用
        [HttpDelete("account/application/{id}")]
        public IActionResult DeleteApplication(string id)
        {
            try
            {
                _applicationService.DeleteApplication(id);
            }
            catch (Exception ex)
            {
                return Json(ApiResponse.Error(-1, ex.Message));
            }

            return Json(ApiResponse.Ok());
        }

        // 添加应用
        [HttpPost("account/application")]
        public IActionResult AddApplication([FromBody] ApplicationCreateDto dto)
        {
            try
            {
                _applicationService.AddApplication(dto);
            }
            catch (Exception ex)
            {
                return Json(ApiResponse.Error(-1, ex.Message));
            }

            return Json(ApiResponse.Ok());
        }

        [HttpPost("account/applications/update-sort")]
        public IActionResult UpdateApplicationsSort([FromBody] List<SortUpdateDto> sortUpdates)
        {
            try
            {
                _applicationService.UpdateApplicationsSort(sortUpdates);
            }
            catch (Exception ex)
            {
                return Json(ApiResponse.Error(-1, ex.Message));
            }
            return Json(ApiResponse.Ok());
        }

        [HttpPost("account/applications/update-sort-by-category/{categoryId}")]
        public IActionResult UpdateApplicationsSortByCategory(string categoryId, [FromBody] List<SortUpdateDto> sortUpdates)
        {
            try
            {
                _applicationService.UpdateApplicationsSortByCategory(categoryId, sortUpdates);
            }
            catch (Exception ex)
            {
                return Json(ApiResponse.Error(-1, ex.Message));
            }
            return Json(ApiResponse.Ok());
        }

        [HttpPost("/account/application-attachment")]
        [DisableRequestSizeLimit] // 禁用请求大小限制，允许大文件上传
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = 3221225472L)]
        public async Task<IActionResult> UploadApplicationAttachment()
        {
            try
            {
                var form = await Request.ReadFormAsync();
                var file = form.Files["file"];
                string fileType = Path.GetExtension(file.FileName).ToLowerInvariant();
                var appName = form["appName"].ToString();
                string fullAppName = appName + fileType;

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "请选择文件" });
                }

                // 临时保存文件的路径
                string localFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "TemporaryFiles");
                // 确保目录存在
                if (!Directory.Exists(localFolderPath))
                    Directory.CreateDirectory(localFolderPath);
                // 临时保存文件在本地
                string localFilePath = Path.Combine(localFolderPath, fullAppName);
                using (var stream = new FileStream(localFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                string smbRoot = @"\\172.16.15.107\SunwinTools";

                // 使用指定账号连接
                string username = @"sunwin"; // 替换为实际用户名
                string password = "sw654321"; // 替换为实际密码

                ConnectHelper connectHelper = new ConnectHelper();
                connectHelper.ConnectState(smbRoot, username, password);
                System.IO.File.Copy(localFilePath, Path.Combine(smbRoot, fullAppName), true);

                // 提取图标
                IconHelper iconHelper = new IconHelper();
                string savePicPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "appLogo");
                string iconPath = iconHelper.ExtractAndSaveIcon(localFilePath, savePicPath, appName);

                // 清理临时文件
                System.IO.File.Delete(localFilePath);

                // 返回结果
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        FileName = file.FileName,
                        FileSize = file.Length,
                        FileType = Path.GetExtension(file.FileName)
                    }
                });
            }
            catch (Exception ex)
            {
                LogHelper.LocalLog("UploadApplicationAttachment", $"上传失败。{JsonConvert.SerializeObject(ex)}");
                return StatusCode(500, new { success = false, message = "上传失败: " + ex.Message });
            }
        }

    }
}

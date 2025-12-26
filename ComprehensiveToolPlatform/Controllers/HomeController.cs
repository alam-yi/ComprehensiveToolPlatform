using ComprehensiveToolPlatform.Common;
using ComprehensiveToolPlatform.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace ComprehensiveToolPlatform.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApplicationService _applicationService;

        public HomeController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        public IActionResult Index()
        {
            var viewModel = _applicationService.GetHomeData();
            return View(viewModel);
        }

        [HttpGet("/home/download")]
        public IActionResult Download([FromQuery] string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return BadRequest("文件名不能为空");
            // 安全过滤文件名
            var safeFileName = Path.GetFileName(filename);
            if (string.IsNullOrEmpty(safeFileName))
                return BadRequest("无效的文件名");

            ConnectHelper connectHelper = new ConnectHelper();
            // 使用指定账号连接
            string username = @"sunwin"; // 替换为实际用户名
            string password = "sw654321"; // 替换为实际密码
            string smbRoot = @"\\172.16.15.107\SunwinTools";
            connectHelper.ConnectState(smbRoot, username, password);

            var uncPath = Path.Combine(smbRoot, safeFileName);

            // 获取文件信息
            var fileInfo = new FileInfo(uncPath);
            var fileSize = fileInfo.Length;

            // 设置响应头
            Response.Headers.Add("Accept-Ranges", "bytes");
            Response.Headers.Add("Content-Disposition",
                $"attachment; filename=\"{Uri.EscapeDataString(safeFileName)}\"");

            // 获取MIME类型
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(safeFileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            // 实现流式传输
            var stream = new FileStream(uncPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 81920, FileOptions.SequentialScan);

            // 返回FileStreamResult，支持范围请求
            return new FileStreamResult(stream, "application/octet-stream")
            {
                FileDownloadName = safeFileName,
                EnableRangeProcessing = true,
                LastModified = fileInfo.LastWriteTimeUtc
            };
        }

        [HttpGet("/home/fileinfo")]
        public IActionResult GetFileInfo(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return BadRequest("文件名不能为空");
            // 安全过滤文件名
            var safeFileName = Path.GetFileName(filename);
            if (string.IsNullOrEmpty(safeFileName))
                return BadRequest("无效的文件名");

            try
            {
                ConnectHelper connectHelper = new ConnectHelper();
                // 使用指定账号连接
                string username = @"sunwin"; // 替换为实际用户名
                string password = "sw654321"; // 替换为实际密码
                string smbRoot = @"\\172.16.15.107\SunwinTools";

                connectHelper.ConnectState(smbRoot, username, password);
                var uncPath = Path.Combine(smbRoot, safeFileName);

                if (!System.IO.File.Exists(uncPath))
                    return BadRequest("文件不存在，请重新加载界面，或联系管理员");

                var fileInfo = new FileInfo(uncPath);

                return Ok(new
                {
                    name = safeFileName,
                    size = fileInfo.Length,
                    lastModified = fileInfo.LastWriteTimeUtc
                });
            }
            catch (Exception ex)
            {
                LogHelper.LocalLog("读取文件信息失败", $"{JsonConvert.SerializeObject(ex)}");
            }
            return BadRequest("获取文件信息失败，请联系管理员");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;

namespace ComprehensiveToolPlatform.Common
{
    public class IconHelper
    {
        /// <summary>
        /// 提取文件图标并保存到指定路径
        /// </summary>
        /// <param name="filePath">目标文件路径</param>
        /// <param name="saveDirectory">保存目录（如 wwwroot/appLogo）</param>
        /// <param name="fileName">保存的文件名（不含扩展名）</param>
        /// <param name="targetSize">目标尺寸（默认60x60）</param>
        /// <returns>保存的文件路径</returns>
        public string ExtractAndSaveIcon(string filePath, string saveDirectory, string fileName = null, int targetSize = 60)
        {
            string savePath = null;
            try
            {
                // 检查文件是否存在
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"文件不存在: {filePath}");

                // 如果未指定文件名，使用原文件名
                if (string.IsNullOrEmpty(fileName))
                    fileName = Path.GetFileNameWithoutExtension(filePath);

                // 确保保存目录存在
                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                // 生成保存路径（PNG格式）
                savePath = Path.Combine(saveDirectory, $"{fileName}.png");

                // 如果文件已存在，先删除
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }

                // 提取图标
                using (Icon icon = Icon.ExtractAssociatedIcon(filePath))
                {
                    if (icon == null)
                        throw new Exception("无法提取图标");

                    // 转换为位图
                    using (Bitmap originalBitmap = icon.ToBitmap())
                    {
                        // 创建新的高分辨率位图
                        using (Bitmap highResBitmap = new Bitmap(targetSize, targetSize, PixelFormat.Format32bppArgb))
                        {
                            // 设置高 DPI 渲染
                            highResBitmap.SetResolution(300, 300);

                            using (Graphics g = Graphics.FromImage(highResBitmap))
                            {
                                // 使用高质量插值算法
                                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                                // 清除背景（透明）
                                g.Clear(Color.Transparent);

                                // 绘制图标
                                g.DrawImage(originalBitmap, 0, 0, targetSize, targetSize);
                            }

                            // 保存图标 - 使用内存流避免文件锁定问题
                            using (MemoryStream memoryStream = new MemoryStream())
                            {
                                // 使用编码器参数提高图片质量
                                var encoderParameters = new EncoderParameters(1);
                                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 100L);

                                // 获取 PNG 编码器
                                var pngCodec = GetEncoderInfo("image/png");

                                if (pngCodec != null)
                                {
                                    highResBitmap.Save(memoryStream, pngCodec, encoderParameters);

                                    // 将内存流写入文件
                                    using (FileStream fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                                    {
                                        memoryStream.WriteTo(fileStream);
                                    }
                                }
                                else
                                {
                                    // 回退到默认保存方式
                                    highResBitmap.Save(savePath, ImageFormat.Png);
                                }
                            }
                        }
                    }
                }

                return savePath;
            }
            catch (Exception ex)
            {
                // 如果保存失败，清理可能创建的部分文件
                if (savePath != null && File.Exists(savePath))
                {
                    try { File.Delete(savePath); } catch { }
                }

                LogHelper.LocalLog("ExtractAndSaveIcon", $"提取图标失败: {JsonConvert.SerializeObject(ex)}");
                throw new Exception($"保存图标失败: {ex.Message}", ex);
            }
        }

        private ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.MimeType == mimeType)
                    return codec;
            }
            return null;
        }

        /// <summary>
        /// 提取图标并返回Base64字符串
        /// </summary>
        public string ExtractIconAsBase64(string filePath)
        {
            try
            {
                using (Icon icon = Icon.ExtractAssociatedIcon(filePath))
                {
                    if (icon == null) return null;

                    using (Bitmap bitmap = icon.ToBitmap())
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bitmap.Save(ms, ImageFormat.Png);
                        byte[] imageBytes = ms.ToArray();
                        return Convert.ToBase64String(imageBytes);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 异步提取并保存图标（可选，避免UI阻塞）
        /// </summary>
        public async Task<string> ExtractAndSaveIconAsync(string filePath, string saveDirectory, string fileName = null, int targetSize = 60)
        {
            return await Task.Run(() => ExtractAndSaveIcon(filePath, saveDirectory, fileName, targetSize));
        }
    }
}
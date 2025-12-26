using System.Diagnostics;
using System.Text;

namespace ComprehensiveToolPlatform.Common
{
    public class ConnectHelper
    {
        // 静态构造函数，在类首次使用时注册编码提供程序
        static ConnectHelper()
        {
            try
            {
                // 尝试注册编码提供程序
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            }
            catch
            {
                // 如果注册失败，可能已经注册过，继续执行
            }
        }

        // 获取适合的编码
        private Encoding GetConsoleEncoding()
        {
            try
            {
                // 先尝试 GB2312
                return Encoding.GetEncoding("GB2312");
            }
            catch
            {
                try
                {
                    // 再尝试代码页 936（GB2312 的代码页）
                    return Encoding.GetEncoding(936);
                }
                catch
                {
                    // 最后回退到 UTF-8
                    return Encoding.UTF8;
                }
            }
        }

        //public bool ConnectState(string path, string userName, string password)
        //{
        //    bool flag = false;
        //    Process proc = null;

        //    try
        //    {
        //        // 先断开所有到同一服务器的现有连接（包括使用其他用户名的连接）
        //        string server = ExtractServerFromPath(path);
        //        DisconnectAllServerConnections(server);

        //        proc = new Process();
        //        proc.StartInfo.FileName = "cmd.exe";
        //        proc.StartInfo.UseShellExecute = false;
        //        proc.StartInfo.RedirectStandardInput = true;
        //        proc.StartInfo.RedirectStandardOutput = true;
        //        proc.StartInfo.RedirectStandardError = true;
        //        proc.StartInfo.CreateNoWindow = true;
        //        proc.StartInfo.StandardOutputEncoding = Encoding.GetEncoding("GB2312");
        //        proc.StartInfo.StandardErrorEncoding = Encoding.GetEncoding("GB2312");
        //        proc.Start();

        //        // 使用正确的net use语法
        //        string dosLine = $"net use \"{path}\" /user:\"{userName}\" \"{password}\" /persistent:yes";
        //        LogHelper.LocalLog("ConnectState", $"dosLine: {dosLine}");
        //        proc.StandardInput.WriteLine(dosLine);
        //        proc.StandardInput.WriteLine("exit");

        //        // 等待命令执行完成
        //        if (!proc.WaitForExit(10000)) // 10秒超时
        //        {
        //            proc.Kill();
        //            throw new TimeoutException("连接命令执行超时");
        //        }

        //        string errorMsg = proc.StandardError.ReadToEnd();
        //        string outputMsg = proc.StandardOutput.ReadToEnd();
        //        LogHelper.LocalLog("ConnectState", $"errorMsg: {errorMsg}");
        //        LogHelper.LocalLog("ConnectState", $"outputMsg: {outputMsg}");

        //        // 检查是否连接成功
        //        if (errorMsg.Contains("1219"))
        //        {
        //            // 再次强制断开所有连接后重试一次
        //            ForceDisconnectAll();
        //            Thread.Sleep(1000); // 等待断开完成

        //            // 使用新的进程重试连接
        //            return RetryConnect(path, userName, password);
        //        }
        //        else if (errorMsg.Contains("1326") || errorMsg.Contains("密码"))
        //        {
        //            throw new Exception($"用户名或密码错误: {errorMsg}");
        //        }
        //        else if (string.IsNullOrEmpty(errorMsg) || outputMsg.Contains("命令成功完成"))
        //        {
        //            flag = true;
        //        }
        //        else
        //        {
        //            throw new Exception($"连接失败: {errorMsg}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.LocalLog("ConnectState Error", $"Path: {path}, User: {userName}, Error: {ex.Message}");
        //        throw;
        //    }
        //    finally
        //    {
        //        proc?.Close();
        //        proc?.Dispose();
        //    }
        //    return flag;
        //}

        public bool ConnectState(string path, string userName, string password)
        {
            bool flag = false;
            Process proc = null;

            try
            {
                // 先断开所有到同一服务器的现有连接（包括使用其他用户名的连接）
                string server = ExtractServerFromPath(path);
                DisconnectAllServerConnections(server);

                proc = new Process();
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;

                // 使用动态获取的编码
                var consoleEncoding = GetConsoleEncoding();
                proc.StartInfo.StandardOutputEncoding = consoleEncoding;
                proc.StartInfo.StandardErrorEncoding = consoleEncoding;

                proc.Start();

                // 使用正确的net use语法
                string dosLine = $"net use \"{path}\" /user:\"{userName}\" \"{password}\" /persistent:yes";
                LogHelper.LocalLog("ConnectState", $"dosLine: {dosLine}");
                proc.StandardInput.WriteLine(dosLine);
                proc.StandardInput.WriteLine("exit");

                // 等待命令执行完成
                if (!proc.WaitForExit(10000)) // 10秒超时
                {
                    proc.Kill();
                    throw new TimeoutException("连接命令执行超时");
                }

                string errorMsg = proc.StandardError.ReadToEnd();
                string outputMsg = proc.StandardOutput.ReadToEnd();
                LogHelper.LocalLog("ConnectState", $"errorMsg: {errorMsg}");
                LogHelper.LocalLog("ConnectState", $"outputMsg: {outputMsg}");

                // 检查是否连接成功
                if (errorMsg.Contains("1219"))
                {
                    // 再次强制断开所有连接后重试一次
                    ForceDisconnectAll();
                    Thread.Sleep(1000); // 等待断开完成

                    // 使用新的进程重试连接
                    return RetryConnect(path, userName, password);
                }
                if (errorMsg.Contains("错误 5") || errorMsg.Contains("ERROR_ACCESS_DENIED"))
                {
                    // 尝试使用IP地址而不是服务器名
                    return ConnectUsingAlternativeMethod(path, userName, password, server);
                }
                else if (errorMsg.Contains("1326") || errorMsg.Contains("密码"))
                {
                    throw new Exception($"用户名或密码错误: {errorMsg}");
                }
                else if (string.IsNullOrEmpty(errorMsg) || outputMsg.Contains("命令成功完成"))
                {
                    flag = true;
                }
                else
                {
                    throw new Exception($"连接失败: {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                LogHelper.LocalLog("ConnectState Error", $"Path: {path}, User: {userName}, Error: {ex.Message}");
                throw;
            }
            finally
            {
                proc?.Close();
                proc?.Dispose();
            }
            return flag;
        }


        // 在连接前启用网络发现
        private void EnableNetworkDiscovery()
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "netsh";
                    process.StartInfo.Arguments = "advfirewall firewall set rule group=\"Network Discovery\" new enable=Yes";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    process.WaitForExit(2000);
                }
            }
            catch { }
        }

        private bool ConnectUsingAlternativeMethod(string path, string userName, string password, string server)
        {
            try
            {
                // 方法1：尝试使用不同的语法
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "net.exe";
                    process.StartInfo.Arguments = $"use \"{path}\" {password} /user:{userName}";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();

                    if (process.WaitForExit(5000))
                    {
                        return process.ExitCode == 0;
                    }
                }

                // 方法2：尝试使用PowerShell
                return ConnectWithPowerShell(path, userName, password);
            }
            catch
            {
                return false;
            }
        }

        private bool ConnectWithPowerShell(string path, string userName, string password)
        {
            try
            {
                using (Process ps = new Process())
                {
                    ps.StartInfo.FileName = "powershell.exe";
                    ps.StartInfo.Arguments = $"-Command \"New-PSDrive -Name 'T' -PSProvider FileSystem -Root '{path}' -Credential (New-Object System.Management.Automation.PSCredential('{userName}', (ConvertTo-SecureString '{password}' -AsPlainText -Force)))\"";
                    ps.StartInfo.UseShellExecute = false;
                    ps.StartInfo.CreateNoWindow = true;
                    ps.Start();

                    return ps.WaitForExit(5000) && ps.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        // 重试连接方法
        private bool RetryConnect(string path, string userName, string password)
        {
            Process proc = null;
            try
            {
                proc = new Process();
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;

                // 使用相同的编码获取方式
                var consoleEncoding = GetConsoleEncoding();
                proc.StartInfo.StandardOutputEncoding = consoleEncoding;
                proc.StartInfo.StandardErrorEncoding = consoleEncoding;

                proc.Start();

                string dosLine = $"net use \"{path}\" /user:\"{userName}\" \"{password}\" /persistent:yes";
                proc.StandardInput.WriteLine(dosLine);
                proc.StandardInput.WriteLine("exit");

                if (!proc.WaitForExit(5000))
                {
                    proc.Kill();
                    return false;
                }

                string outputMsg = proc.StandardOutput.ReadToEnd();
                return outputMsg.Contains("命令成功完成");
            }
            catch
            {
                return false;
            }
            finally
            {
                proc?.Close();
                proc?.Dispose();
            }
        }

        // 断开指定服务器的所有连接（包括IPC$和其他共享）
        private void DisconnectAllServerConnections(string server)
        {
            try
            {
                // 使用net session命令查看当前会话
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.StandardOutputEncoding = Encoding.GetEncoding("GB2312");
                    process.Start();

                    // 查看到指定服务器的所有连接
                    process.StandardInput.WriteLine($"net use | findstr /i \"{server}\"");
                    process.StandardInput.WriteLine("exit");

                    if (!process.WaitForExit(3000))
                    {
                        process.Kill();
                        return;
                    }

                    string output = process.StandardOutput.ReadToEnd();

                    // 解析输出，断开每个找到的连接
                    if (!string.IsNullOrEmpty(output))
                    {
                        string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                        foreach (string line in lines)
                        {
                            if (line.Contains(server))
                            {
                                // 提取共享路径
                                string sharePath = ExtractSharePathFromLine(line, server);
                                if (!string.IsNullOrEmpty(sharePath))
                                {
                                    DisconnectSingleShare(sharePath);
                                }
                            }
                        }
                    }
                }

                // 额外断开IPC$连接（可能不会在net use中显示）
                DisconnectSingleShare($"\\\\{server}\\IPC$");
            }
            catch (Exception ex)
            {
                LogHelper.LocalLog("DisconnectAllServerConnections Error", ex.Message);
            }
        }

        // 从输出行中提取共享路径
        private string ExtractSharePathFromLine(string line, string server)
        {
            try
            {
                // 查找服务器名称后的共享路径
                int serverIndex = line.IndexOf($"\\\\{server}\\");
                if (serverIndex >= 0)
                {
                    int endIndex = line.IndexOf(' ', serverIndex);
                    if (endIndex > serverIndex)
                    {
                        return line.Substring(serverIndex, endIndex - serverIndex).Trim();
                    }
                    else
                    {
                        return line.Substring(serverIndex).Trim();
                    }
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        // 断开单个共享连接
        private void DisconnectSingleShare(string sharePath)
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();

                    process.StandardInput.WriteLine($"net use \"{sharePath}\" /delete /y");
                    process.StandardInput.WriteLine("exit");
                    process.WaitForExit(2000);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LocalLog("DisconnectSingleShare Error", $"Share: {sharePath}, Error: {ex.Message}");
            }
        }

        // 强制断开所有连接
        private void ForceDisconnectAll()
        {
            try
            {
                // 先断开所有网络映射
                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = "cmd.exe";
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardInput = true;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.Start();

                    proc.StandardInput.WriteLine("net use * /delete /y");
                    proc.StandardInput.WriteLine("exit");
                    proc.WaitForExit(3000);
                }

                // 清除可能的残留会话
                Thread.Sleep(500);

                // 使用net session命令清除会话
                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = "cmd.exe";
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardInput = true;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.Start();

                    proc.StandardInput.WriteLine("net session /delete /y");
                    proc.StandardInput.WriteLine("exit");
                    proc.WaitForExit(3000);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LocalLog("ForceDisconnectAll Error", ex.Message);
            }
        }

        // 从路径中提取服务器地址
        private string ExtractServerFromPath(string path)
        {
            try
            {
                if (path.StartsWith(@"\\"))
                {
                    string temp = path.Substring(2);
                    int endIndex = temp.IndexOf('\\');
                    if (endIndex > 0)
                    {
                        return temp.Substring(0, endIndex);
                    }
                    return temp;
                }
                return path;
            }
            catch
            {
                return path;
            }
        }
    }
}
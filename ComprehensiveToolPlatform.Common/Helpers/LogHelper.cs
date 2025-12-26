namespace ComprehensiveToolPlatform.Common
{
    public class LogHelper
    {
        public static void LocalLog(string flag, string log)
        {
            string fileDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(fileDir)) Directory.CreateDirectory(fileDir);
            File.AppendAllText($@"{fileDir}\log_{DateTime.Now:yyyyMMdd}.txt", $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] -T {Thread.CurrentThread.ManagedThreadId}  [{flag}] {log}\r\n");
        }
    }
}

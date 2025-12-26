namespace ComprehensiveToolPlatform.Common
{
    public class BusinessException : Exception
    {
        public int ErrorCode { get; }

        public BusinessException(int errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }

    // 预定义业务异常
    public static class BusinessErrors
    {
        public static BusinessException UserNotFound => new BusinessException(1001, "User not found");
        public static BusinessException InvalidParameter => new BusinessException(1002, "Invalid parameter");
        public static BusinessException InsufficientBalance => new BusinessException(1003, "Insufficient balance");
        // 添加更多业务异常...
    }
}

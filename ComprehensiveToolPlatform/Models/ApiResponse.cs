namespace ComprehensiveToolPlatform
{
    public class ApiResponse<T>
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public bool Success => Code == 200;

        public static ApiResponse<T> Ok(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Code = 200,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> Error(int code, string message)
        {
            return new ApiResponse<T>
            {
                Code = code,
                Message = message,
                Data = default
            };
        }
    }

    // 无数据返回的简化版本
    public class ApiResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public bool Success => Code == 200;

        public static ApiResponse Ok(string message = "Success")
        {
            return new ApiResponse
            {
                Code = 200,
                Message = message
            };
        }

        public static ApiResponse Error(int code, string message)
        {
            return new ApiResponse
            {
                Code = code,
                Message = message
            };
        }
    }
}

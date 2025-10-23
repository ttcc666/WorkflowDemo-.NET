namespace WorkFlowDemo.Models.Common
{
    public class ApiResponse
    {
        public int Code { get; set; } = 200;
        public string Message { get; set; } = "Success";

        public static ApiResponse Success()
        {
            return new ApiResponse();
        }

        public static ApiResponse<T> Success<T>(T data)
        {
            return new ApiResponse<T> { Data = data };
        }

        public static ApiResponse Fail(string message, int code = 500)
        {
            return new ApiResponse { Code = code, Message = message };
        }

        public static ApiResponse<T> Fail<T>(string message, int code = 500)
        {
            return new ApiResponse<T> { Code = code, Message = message };
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T? Data { get; set; }
    }
}
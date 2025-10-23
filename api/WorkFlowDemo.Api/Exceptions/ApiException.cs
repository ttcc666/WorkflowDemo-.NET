namespace WorkFlowDemo.Api.Exceptions
{
    public class ApiException : Exception
    {
        public int Code { get; }

        public ApiException(string message, int code = 500) : base(message)
        {
            Code = code;
        }
    }
}
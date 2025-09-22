using System.Collections.Generic;

namespace FinTech.Application.Common
{
    public class BaseResponse<T>
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }
        public T Data { get; set; }

        public BaseResponse(bool succeeded, string message, T data = default, List<string> errors = null)
        {
            Succeeded = succeeded;
            Message = message;
            Data = data;
            Errors = errors;
        }

        public static BaseResponse<T> Success(T data, string message = "Request successful.")
        {
            return new BaseResponse<T>(true, message, data);
        }

        public static BaseResponse<T> Failure(string message, List<string> errors = null)
        {
            return new BaseResponse<T>(false, message, default, errors);
        }
    }
}

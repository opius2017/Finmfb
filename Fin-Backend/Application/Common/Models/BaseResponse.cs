using System;
using System.Collections.Generic;

namespace FinTech.Application.Common.Models
{
    public class BaseResponse<T>
    {
        public T? Data { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }

        public BaseResponse()
        {
            Errors = new List<string>();
            Message = string.Empty;
        }

        public BaseResponse(T? data, bool success = true, string message = "")
        {
            Data = data;
            Success = success;
            Message = message;
            Errors = new List<string>();
        }

        public BaseResponse(string message, bool success = false)
        {
            Data = default(T);
            Success = success;
            Message = message;
            Errors = new List<string> { message };
        }

        public static BaseResponse<T> SuccessResponse(T data, string message = "Success")
        {
            return new BaseResponse<T>(data, true, message);
        }

        public static BaseResponse<T> ErrorResponse(string message, List<string>? errors = null)
        {
            return new BaseResponse<T>(default(T), false, message)
            {
                Errors = errors ?? new List<string> { message }
            };
        }
        
        public static BaseResponse<T> Failure(string message)
        {
            return new BaseResponse<T>(message, false);
        }
    }
}

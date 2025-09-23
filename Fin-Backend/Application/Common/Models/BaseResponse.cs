using System;
using System.Collections.Generic;

namespace FinTech.Application.Common.Models
{
    public class BaseResponse<T>
    {
        public T Data { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }

        public BaseResponse(T data, bool success = true, string message = "")
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
            Errors = new List<string>();
        }
    }
}

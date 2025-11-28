using System;
using System.Collections.Generic;
using System.Linq;

namespace FinTech.Shared.Common
{
    /// <summary>
    /// Represents the result of an operation with success/failure status and optional value/errors
    /// </summary>
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T Data { get; }
        public string Message { get; }
        public List<Error> Errors { get; }
        public int StatusCode { get; }

        protected Result(bool isSuccess, T data, string message, List<Error> errors, int statusCode = 200)
        {
            IsSuccess = isSuccess;
            Data = data;
            Message = message;
            Errors = errors ?? new List<Error>();
            StatusCode = statusCode;
        }

        public static Result<T> Success(T data, string message = "Operation successful", int statusCode = 200)
            => new(true, data, message, new List<Error>(), statusCode);

        public static Result<T> Failure(string message, List<Error> errors = null, int statusCode = 400)
            => new(false, default, message, errors ?? new List<Error>(), statusCode);

        public static Result<T> Failure(string message, Error error, int statusCode = 400)
            => Failure(message, new List<Error> { error }, statusCode);

        public static Result<T> NotFound(string message = "Resource not found")
            => new(false, default, message, new List<Error> { new("NOT_FOUND", message) }, 404);

        public static Result<T> Unauthorized(string message = "Unauthorized")
            => new(false, default, message, new List<Error> { new("UNAUTHORIZED", message) }, 401);

        public static Result<T> Forbidden(string message = "Forbidden")
            => new(false, default, message, new List<Error> { new("FORBIDDEN", message) }, 403);

        public static Result<T> ValidationFailed(List<Error> errors)
            => new(false, default, "Validation failed", errors, 422);

        public static Result<T> Conflict(string message = "Resource conflict")
            => new(false, default, message, new List<Error> { new("CONFLICT", message) }, 409);
    }

    /// <summary>
    /// Non-generic Result for void operations
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; }
        public string Message { get; }
        public List<Error> Errors { get; }
        public int StatusCode { get; }

        protected Result(bool isSuccess, string message, List<Error> errors, int statusCode = 200)
        {
            IsSuccess = isSuccess;
            Message = message;
            Errors = errors ?? new List<Error>();
            StatusCode = statusCode;
        }

        public static Result Success(string message = "Operation successful", int statusCode = 200)
            => new(true, message, new List<Error>(), statusCode);

        public static Result Failure(string message, List<Error> errors = null, int statusCode = 400)
            => new(false, message, errors ?? new List<Error>(), statusCode);

        public static Result Failure(string message, Error error, int statusCode = 400)
            => Failure(message, new List<Error> { error }, statusCode);

        public static Result NotFound(string message = "Resource not found")
            => new(false, message, new List<Error> { new("NOT_FOUND", message) }, 404);

        public static Result Unauthorized(string message = "Unauthorized")
            => new(false, message, new List<Error> { new("UNAUTHORIZED", message) }, 401);

        public static Result Forbidden(string message = "Forbidden")
            => new(false, message, new List<Error> { new("FORBIDDEN", message) }, 403);

        public static Result ValidationFailed(List<Error> errors)
            => new(false, "Validation failed", errors, 422);
    }

    /// <summary>
    /// Represents an error with code and message
    /// </summary>
    public class Error
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Field { get; set; }

        public Error(string code, string message, string field = null)
        {
            Code = code;
            Message = message;
            Field = field;
        }
    }
}

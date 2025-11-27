namespace FinTech.Core.Application.Common.Models
{
    /// <summary>
    /// Represents an error with code and message
    /// </summary>
    public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Failure)
    {
        public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);
        public static readonly Error NullValue = new("Error.NullValue", "Null value was provided", ErrorType.Validation);

        public static Error Validation(string code, string message) =>
            new(code, message, ErrorType.Validation);

        public static Error NotFound(string code, string message) =>
            new(code, message, ErrorType.NotFound);

        public static Error Conflict(string code, string message) =>
            new(code, message, ErrorType.Conflict);

        public static Error Failure(string code, string message) =>
            new(code, message, ErrorType.Failure);

        public static Error Unauthorized(string code, string message) =>
            new(code, message, ErrorType.Unauthorized);

        public static Error Forbidden(string code, string message) =>
            new(code, message, ErrorType.Forbidden);
    }

    /// <summary>
    /// Error type enumeration
    /// </summary>
    public enum ErrorType
    {
        None = 0,
        Failure = 1,
        Validation = 2,
        NotFound = 3,
        Conflict = 4,
        Unauthorized = 5,
        Forbidden = 6
    }
}

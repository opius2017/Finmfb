using System.Collections.Generic;

namespace FinTech.Core.Application.Common.Models
{
    /// <summary>
    /// Represents a validation error with multiple field errors
    /// </summary>
    public sealed class ValidationError
    {
        public ValidationError(Error[] errors)
        {
            Code = "Validation.General";
            Message = "One or more validation errors occurred";
            Type = ErrorType.Validation;
            Errors = errors;
        }

        public string Code { get; }
        public string Message { get; }
        public ErrorType Type { get; }
        public Error[] Errors { get; }

        public static ValidationError FromResults(IEnumerable<Result> results)
        {
            var errors = new List<Error>();
            foreach (var result in results)
            {
                if (result.IsFailure)
                {
                    errors.Add(result.Error);
                }
            }
            return new ValidationError(errors.ToArray());
        }
    }
}

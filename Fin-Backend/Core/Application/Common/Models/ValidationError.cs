using System.Collections.Generic;

namespace FinTech.Core.Application.Common.Models
{
    /// <summary>
    /// Represents a validation error with multiple field errors
    /// </summary>
    public sealed class ValidationError : Error
    {
        public ValidationError(Error[] errors)
            : base("Validation.General", "One or more validation errors occurred", ErrorType.Validation)
        {
            Errors = errors;
        }

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

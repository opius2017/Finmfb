using System.Collections.Generic;
using System.Linq;

namespace FinTech.Core.Application.Common.Models
{
    /// <summary>
    /// Represents a validation error with multiple field errors
    /// </summary>
    public sealed class ValidationError
    {
        public ValidationError(object[] errors)
        {
            Errors = errors;
            Message = "One or more validation errors occurred";
        }

        public object[] Errors { get; }

        public string Message { get; }

        public static ValidationError FromResults(IEnumerable<dynamic> results)
        {
            var errors = results
                .Where(r => r.IsFailure)
                .Select(r => r.Error)
                .ToArray();
            return new ValidationError(errors);
        }
    }
}

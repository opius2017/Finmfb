using System;

namespace FinTech.Core.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when attempting to create an entity that already exists
    /// </summary>
    public class DuplicateEntityException : Exception
    {
        public DuplicateEntityException() : base() { }
        
        public DuplicateEntityException(string message) : base(message) { }
        
        public DuplicateEntityException(string message, Exception innerException) : base(message, innerException) { }
    }
}

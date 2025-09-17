using System;

namespace FinTech.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested entity is not found
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException() : base() { }
        
        public NotFoundException(string message) : base(message) { }
        
        public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
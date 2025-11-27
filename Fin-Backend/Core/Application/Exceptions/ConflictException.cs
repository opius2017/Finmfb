namespace FinTech.Core.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when a conflict occurs
    /// </summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message)
            : base(message)
        {
        }

        public ConflictException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

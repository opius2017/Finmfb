namespace FinTech.Core.Application.Interfaces.Services;

/// <summary>
/// Service for getting current date and time (useful for testing)
/// </summary>
public interface IDateTimeService
{
    DateTime UtcNow { get; }
    DateTime Now { get; }
    DateOnly Today { get; }
}

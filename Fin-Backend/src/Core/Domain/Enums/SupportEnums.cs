namespace FinTech.Domain.Enums
{
    public enum TicketStatus
    {
        Open,
        InProgress,
        Resolved,
        Closed,
        Cancelled
    }

    public enum TicketPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum SenderType
    {
        Customer,
        SupportAgent,
        System
    }
}
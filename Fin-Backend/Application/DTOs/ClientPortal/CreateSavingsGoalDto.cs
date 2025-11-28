using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class CreateSavingsGoalDto
    {
        public string GoalName { get; set; }
        public string Description { get; set; }
        public decimal TargetAmount { get; set; }
        public string Currency { get; set; } = "NGN";
        public DateTime StartDate { get; set; }
        public DateTime TargetDate { get; set; }
        public string GoalCategory { get; set; }
        public string RecurrencePattern { get; set; }
        public decimal AutoTransferAmount { get; set; }
        public string SourceAccountNumber { get; set; }
        public string DestinationAccountNumber { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class SavingsGoalDto
    {
        public Guid Id { get; set; }
        public string GoalName { get; set; }
        public string Description { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public string Currency { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime TargetDate { get; set; }
        public string GoalCategory { get; set; }
        public string Status { get; set; }
        public string RecurrencePattern { get; set; }
        public decimal AutoTransferAmount { get; set; }
        public string SourceAccountNumber { get; set; }
        public string DestinationAccountNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<SavingsGoalTransactionDto> Transactions { get; set; }
    }
}

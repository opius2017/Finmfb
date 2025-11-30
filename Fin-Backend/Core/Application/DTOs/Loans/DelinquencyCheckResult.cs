using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class DelinquencyCheckResult
    {
        public DateTime CheckDate { get; set; }
        public int LoansChecked { get; set; }
        public int NewDelinquencies { get; set; }
        public int PenaltiesApplied { get; set; }
        public int NotificationsSent { get; set; }
        public List<DelinquentLoanSummary> DelinquentLoans { get; set; } = new List<DelinquentLoanSummary>();
    }
}

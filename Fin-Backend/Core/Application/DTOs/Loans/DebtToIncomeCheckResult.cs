using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class DebtToIncomeCheckResult
    {
        public bool IsEligible { get; set; }
        public List<string> Reasons { get; set; } = new List<string>();
        
        public decimal MonthlyIncome { get; set; }
        public decimal ExistingDebtPayments { get; set; }
        public decimal ProposedEMI { get; set; }
        public decimal TotalDebtPayments { get; set; }
        public decimal DTIRatio { get; set; }
        public decimal MaxAllowedDTI { get; set; }
    }
}

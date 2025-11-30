using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class DelinquencyReport
    {
        public DateTime ReportDate { get; set; }
        public int TotalOverdueLoans { get; set; }
        public decimal TotalOverdueAmount { get; set; }
        public decimal TotalPenalties { get; set; }
        
        // By Days Overdue
        public int OverdueBy1to7Days { get; set; }
        public int OverdueBy8to30Days { get; set; }
        public int OverdueBy31to90Days { get; set; }
        public int OverdueBy91PlusDays { get; set; }
        
        // By Classification
        public int PerformingLoans { get; set; }
        public int SpecialMentionLoans { get; set; }
        public int SubstandardLoans { get; set; }
        public int DoubtfulLoans { get; set; }
        public int LossLoans { get; set; }
        
        public List<OverdueLoan> OverdueLoans { get; set; } = new List<OverdueLoan>();
    }
}

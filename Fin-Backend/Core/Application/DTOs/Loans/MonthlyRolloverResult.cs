using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class MonthlyRolloverResult
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int ProcessedCount { get; set; }
        public List<string> ProcessedApplications { get; set; } = new List<string>();
        public string Message { get; set; }
    }
}

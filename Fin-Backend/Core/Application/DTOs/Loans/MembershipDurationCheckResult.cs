using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Loans
{
    public class MembershipDurationCheckResult
    {
        public bool IsEligible { get; set; }
        public List<string> Reasons { get; set; } = new List<string>();
        
        public int MembershipMonths { get; set; }
        public int RequiredMonths { get; set; }
        public DateTime MemberSince { get; set; }
    }
}

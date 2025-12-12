using System;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class ClientDashboardDto
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalBalance { get; set; }
        public int TotalAccounts { get; set; }
        public int TotalLoans { get; set; }
        public decimal TotalLoanBalance { get; set; }
    }
}

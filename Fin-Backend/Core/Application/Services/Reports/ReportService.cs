using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using FinTech.Core.Application.Interfaces.Reports;
using FinTech.Core.Application.DTOs.Reports;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.Interfaces.Repositories.Loans;
using FinTech.Core.Domain.Entities.Loans;

namespace FinTech.Core.Application.Services.Reports
{
    public class ReportService : IReportService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IApplicationDbContext _context;

        public ReportService(ILoanRepository loanRepository, IApplicationDbContext context)
        {
            _loanRepository = loanRepository;
            _context = context;
        }

        public async Task<PortfolioReportDto> GetPortfolioReportAsync(DateTime? asOfDate)
        {
            var loans = await _loanRepository.GetAllAsync();
            var activeLoans = loans.Where(l => l.Status == "ACTIVE").ToList();

            var totalValue = activeLoans.Sum(l => l.PrincipalAmount);
            
            return new PortfolioReportDto
            {
                TotalPortfolioValue = totalValue,
                TotalActiveLoans = activeLoans.Count,
                PortfolioAtRisk = 0, // Placeholder
                Par30 = 0,
                Par60 = 0,
                Par90 = 0
            };
        }

        public async Task<byte[]> GenerateGeneralReportAsync(ReportRequestDto request)
        {
             return new byte[0];
        }

        public async Task<DashboardMetricsDto> GetDashboardMetricsAsync()
        {
             var activeCustomers = await _context.Users.CountAsync(u => u.IsActive);
             
             return new DashboardMetricsDto
             {
                 ActiveCustomers = activeCustomers,
                 PendingApplications = 0,
                 TotalDisbursed = 0
             };
        }
    }
}

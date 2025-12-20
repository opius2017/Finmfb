using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using FinTech.Core.Application.DTOs.Reports;

namespace FinTech.Core.Application.Interfaces.Reports
{
    public interface IReportService
    {
        Task<PortfolioReportDto> GetPortfolioReportAsync(DateTime? asOfDate);
        Task<byte[]> GenerateGeneralReportAsync(ReportRequestDto request);
        Task<DashboardMetricsDto> GetDashboardMetricsAsync();
    }
}

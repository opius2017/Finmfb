using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinTech.Core.Application.Interfaces.Reports;
using FinTech.Core.Application.DTOs.Reports;

namespace FinTech.Controllers.Reports
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("portfolio")]
        [ProducesResponseType(typeof(PortfolioReportDto), 200)]
        public async Task<IActionResult> GetPortfolioReport([FromQuery] DateTime? asOfDate)
        {
            var report = await _reportService.GetPortfolioReportAsync(asOfDate);
            return Ok(report);
        }

        [HttpPost("generate")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        public async Task<IActionResult> GenerateReport([FromBody] ReportRequestDto request)
        {
            var fileContent = await _reportService.GenerateGeneralReportAsync(request);
            // Assuming PDF for now
            return File(fileContent, "application/pdf", $"report_{DateTime.Now:yyyyMMdd}.pdf");
        }
        
        [HttpGet("metrics")]
        [ProducesResponseType(typeof(DashboardMetricsDto), 200)]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            var metrics = await _reportService.GetDashboardMetricsAsync();
            return Ok(metrics);
        }
    }
}

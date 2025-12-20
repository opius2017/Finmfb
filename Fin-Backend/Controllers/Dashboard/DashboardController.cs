using System;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTech.Controllers.Dashboard
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
        {
            var result = await _dashboardService.GetOverviewAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("executive")]
        public async Task<IActionResult> GetExecutive(CancellationToken cancellationToken)
        {
            var result = await _dashboardService.GetExecutiveDashboardAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("loans")]
        public async Task<IActionResult> GetLoans(CancellationToken cancellationToken)
        {
            var result = await _dashboardService.GetLoanDashboardAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("deposits")]
        public async Task<IActionResult> GetDeposits(CancellationToken cancellationToken)
        {
            var result = await _dashboardService.GetDepositDashboardAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory(CancellationToken cancellationToken)
        {
            var result = await _dashboardService.GetInventoryDashboardAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("payroll")]
        public async Task<IActionResult> GetPayroll(CancellationToken cancellationToken)
        {
            var result = await _dashboardService.GetPayrollDashboardAsync(cancellationToken);
            return Ok(result);
        }
    }
}

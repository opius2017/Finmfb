using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FinTech.Application.Services;

namespace FinTech.Presentation.WebAPI.Controllers
{
    [ApiController]
    [Route("api/deposit-operations")]
    [Authorize]
    public class DepositOperationsController : ControllerBase
    {
        private readonly IDepositSweepService _sweepService;
        private readonly IDormancyTrackingService _dormancyService;

        public DepositOperationsController(IDepositSweepService sweepService, IDormancyTrackingService dormancyService)
        {
            _sweepService = sweepService;
            _dormancyService = dormancyService;
        }

        [HttpPost("run-sweeps")]
        public async Task<ActionResult<List<DepositSweepResult>>> RunAutomatedSweeps()
        {
            var tenantId = GetTenantId();
            var results = await _sweepService.RunAutomatedSweepsAsync(tenantId);
            return Ok(results);
        }

        [HttpPost("track-dormancy")]
        public async Task<ActionResult<List<DormancyTrackingResult>>> TrackDormantAccounts([FromQuery] int dormancyDays = 90)
        {
            var tenantId = GetTenantId();
            var results = await _dormancyService.TrackDormantAccountsAsync(tenantId, dormancyDays);
            return Ok(results);
        }

        private Guid GetTenantId()
        {
            var tenantIdClaim = User.Claims.FirstOrDefault(c => c.Type == "TenantId")?.Value;
            return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : Guid.Empty;
        }
    }
}

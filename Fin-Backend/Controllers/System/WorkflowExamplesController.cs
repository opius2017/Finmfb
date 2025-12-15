using System.Threading.Tasks;
using FinTech.Examples;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FinTech.Controllers.System
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowExamplesController : ControllerBase
    {
        private readonly ILogger<WorkflowExamplesController> _logger;
        private readonly BankingTransactionWorkflow _bankingWorkflow;
        private readonly LoanProcessingWorkflow _loanWorkflow;
        private readonly PayrollProcessingWorkflow _payrollWorkflow;
        private readonly FixedAssetWorkflow _fixedAssetWorkflow;

        public WorkflowExamplesController(
            ILogger<WorkflowExamplesController> logger,
            BankingTransactionWorkflow bankingWorkflow,
            LoanProcessingWorkflow loanWorkflow,
            PayrollProcessingWorkflow payrollWorkflow,
            FixedAssetWorkflow fixedAssetWorkflow)
        {
            _logger = logger;
            _bankingWorkflow = bankingWorkflow;
            _loanWorkflow = loanWorkflow;
            _payrollWorkflow = payrollWorkflow;
            _fixedAssetWorkflow = fixedAssetWorkflow;
        }

        [HttpPost("banking")]
        public async Task<IActionResult> RunBankingWorkflow()
        {
            _logger.LogInformation("Executing banking workflow example");
            await _bankingWorkflow.RunDepositWorkflowAsync();
            return Ok(new { message = "Banking workflow executed successfully" });
        }

        [HttpPost("loan")]
        public async Task<IActionResult> RunLoanWorkflow()
        {
            _logger.LogInformation("Executing loan workflow example");
            await _loanWorkflow.RunLoanLifecycleWorkflowAsync();
            return Ok(new { message = "Loan workflow executed successfully" });
        }

        [HttpPost("payroll")]
        public async Task<IActionResult> RunPayrollWorkflow()
        {
            _logger.LogInformation("Executing payroll workflow example");
            await _payrollWorkflow.RunPayrollCycleWorkflowAsync();
            return Ok(new { message = "Payroll workflow executed successfully" });
        }

        [HttpPost("fixed-asset")]
        public async Task<IActionResult> RunFixedAssetWorkflow()
        {
            _logger.LogInformation("Executing fixed asset workflow example");
            await _fixedAssetWorkflow.RunFixedAssetLifecycleWorkflowAsync();
            return Ok(new { message = "Fixed asset workflow executed successfully" });
        }
    }
}


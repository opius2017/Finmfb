using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.Loans;
using FinTech.Core.Application.Interfaces.Loans;
using FinTech.Core.Domain.Entities.Loans;

namespace FinTech.Core.Application.Services.Loans
{
    public class LoanProvisioningService : ILoanProvisioningService
    {
        // In a real implementation, inject repositories and other dependencies
        private static readonly List<LoanProvisioning> _provisionings = new();

        public async Task<LoanProvisioningDto> CalculateProvisioningAsync(string loanId)
        {
            // Dummy logic for expected credit loss calculation
            var ecl = new Random().Next(1000, 100000); // Replace with real calculation
            var provisioning = new LoanProvisioning
            {
                Id = Guid.NewGuid().ToString(),
                LoanId = loanId,
                ExpectedCreditLoss = ecl,
                CalculationDate = DateTime.UtcNow,
                Stage = 1,
                Notes = "Calculated by dummy logic"
            };
            _provisionings.Add(provisioning);
            return await Task.FromResult(new LoanProvisioningDto
            {
                LoanId = provisioning.LoanId,
                ExpectedCreditLoss = provisioning.ExpectedCreditLoss,
                CalculationDate = provisioning.CalculationDate,
                Stage = provisioning.Stage.ToString(),
                Notes = provisioning.Notes
            });
        }

        public async Task<IEnumerable<LoanProvisioningDto>> GetAllProvisioningsAsync()
        {
            return await Task.FromResult(_provisionings.Select(p => new LoanProvisioningDto
            {
                LoanId = p.LoanId ?? string.Empty,
                ExpectedCreditLoss = p.ExpectedCreditLoss,
                CalculationDate = p.CalculationDate,
                Stage = p.Stage.ToString(),
                Notes = p.Notes ?? string.Empty
            }));
        }

        public async Task<LoanProvisioningDto> GetProvisioningByLoanIdAsync(string loanId)
        {
            var provisioning = _provisionings.FirstOrDefault(p => p.LoanId == loanId);
            if (provisioning == null)
            {
                return await Task.FromResult(new LoanProvisioningDto
                {
                    LoanId = loanId,
                    ExpectedCreditLoss = 0,
                    CalculationDate = DateTime.MinValue,
                    Stage = "N/A",
                    Notes = "No provisioning found"
                });
            }
            return await Task.FromResult(new LoanProvisioningDto
            {
                LoanId = provisioning.LoanId ?? string.Empty,
                ExpectedCreditLoss = provisioning.ExpectedCreditLoss,
                CalculationDate = provisioning.CalculationDate,
                Stage = provisioning.Stage.ToString(),
                Notes = provisioning.Notes ?? string.Empty
            });
        }
    }
}

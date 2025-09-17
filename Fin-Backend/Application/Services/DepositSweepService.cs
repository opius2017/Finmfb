using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Domain.Entities.Deposits;
using FinTech.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinTech.Application.Services
{
    public interface IDepositSweepService
    {
        Task<List<DepositSweepResult>> RunAutomatedSweepsAsync(Guid tenantId);
    }

    public class DepositSweepResult
    {
        public string AccountNumber { get; set; } = string.Empty;
        public decimal AmountSwept { get; set; }
        public string SweepType { get; set; } = string.Empty;
        public string Status { get; set; } = "Success";
        public string? Message { get; set; }
    }

    public class DepositSweepService : IDepositSweepService
    {
        private readonly IApplicationDbContext _dbContext;
        public DepositSweepService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<DepositSweepResult>> RunAutomatedSweepsAsync(Guid tenantId)
        {
            var results = new List<DepositSweepResult>();
            var accounts = await _dbContext.DepositAccounts
                .Include(a => a.Product)
                .Where(a => a.TenantId == tenantId && a.Status == Domain.Enums.AccountStatus.Active)
                .ToListAsync();

            foreach (var account in accounts)
            {
                // Example: Sweep excess balance to investment if above threshold
                if (account.Balance > account.Product.MaximumBalance && account.Product.MaximumBalance > 0)
                {
                    var sweptAmount = account.Balance - account.Product.MaximumBalance;
                    account.Balance = account.Product.MaximumBalance;
                    // TODO: Create investment transaction, update ledgers, etc.
                    results.Add(new DepositSweepResult
                    {
                        AccountNumber = account.AccountNumber,
                        AmountSwept = sweptAmount,
                        SweepType = "Excess Balance",
                        Status = "Success",
                        Message = $"Swept {sweptAmount} from account {account.AccountNumber}"
                    });
                }
                // Add more sweep rules as needed
            }
            await _dbContext.SaveChangesAsync();
            return results;
        }
    }
}

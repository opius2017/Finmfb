using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Domain.Entities.Deposits;
using FinTech.Core.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinTech.Application.Services
{
    public interface IDormancyTrackingService
    {
        Task<List<DormancyTrackingResult>> TrackDormantAccountsAsync(Guid tenantId, int dormancyDays = 90);
    }

    public class DormancyTrackingResult
    {
        public string AccountNumber { get; set; } = string.Empty;
        public DateTime? LastTransactionDate { get; set; }
        public string Status { get; set; } = "Active";
        public string? Message { get; set; }
    }

    public class DormancyTrackingService : IDormancyTrackingService
    {
        private readonly IApplicationDbContext _dbContext;
        public DormancyTrackingService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<DormancyTrackingResult>> TrackDormantAccountsAsync(Guid tenantId, int dormancyDays = 90)
        {
            var results = new List<DormancyTrackingResult>();
            var cutoffDate = DateTime.UtcNow.AddDays(-dormancyDays);
            var accounts = await _dbContext.DepositAccounts
                .Where(a => a.TenantId == tenantId && a.Status == Domain.Enums.AccountStatus.Active)
                .ToListAsync();

            foreach (var account in accounts)
            {
                if (account.LastTransactionDate == null || account.LastTransactionDate < cutoffDate)
                {
                    account.Status = Domain.Enums.AccountStatus.Dormant;
                    results.Add(new DormancyTrackingResult
                    {
                        AccountNumber = account.AccountNumber,
                        LastTransactionDate = account.LastTransactionDate,
                        Status = "Dormant",
                        Message = $"Account {account.AccountNumber} marked dormant."
                    });
                }
            }
            await _dbContext.SaveChangesAsync();
            return results;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Domain.Entities.Customers;
using FinTech.Core.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinTech.Core.Application.Services
{
    public interface IRiskScoringService
    {
        Task<RiskProfileDto> ScoreCustomerRiskAsync(Guid customerId);
    }

    public class RiskScoringService : IRiskScoringService
    {
        private readonly IApplicationDbContext _dbContext;
        public RiskScoringService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RiskProfileDto> ScoreCustomerRiskAsync(Guid customerId)
        {
            var customer = await _dbContext.Customers.FirstOrDefaultAsync(c => c.Id == customerId.ToString());
            if (customer == null)
                throw new KeyNotFoundException($"Customer {customerId} not found");

            // Example ML logic: Use KYC, account status, loan history, etc.
            var factors = new List<string>();
            decimal score = 0.1m;
            string riskLevel = "Low";

            if (customer.Status != Domain.Enums.CustomerStatus.Active)
            {
                score += 0.3m;
                factors.Add("Inactive or non-standard status");
            }
            if (customer.RiskRating == Domain.Enums.RiskRating.High)
            {
                score += 0.5m;
                riskLevel = "High";
                factors.Add("High risk rating");
            }
            if (customer.LastKYCUpdateDate == null || (DateTime.UtcNow - customer.LastKYCUpdateDate.Value).TotalDays > 365)
            {
                score += 0.2m;
                factors.Add("KYC outdated or missing");
            }
            // TODO: Add more ML features (loan delinquencies, fraud flags, etc.)

            return new RiskProfileDto
            {
                CustomerId = customerId,
                RiskLevel = riskLevel,
                RiskScore = score,
                Factors = factors.ToArray(),
                EvaluatedAt = DateTime.UtcNow
            };
        }
    }
}

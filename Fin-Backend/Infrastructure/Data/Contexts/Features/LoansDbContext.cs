using Microsoft.EntityFrameworkCore;
using FinTech.Core.Domain.Features.Loans.Entities;

namespace FinTech.Infrastructure.Data.Contexts.Features
{
    /// <summary>
    /// Database context for Loan Management feature module
    /// </summary>
    public class LoansDbContext : DbContext
    {
        public LoansDbContext(DbContextOptions<LoansDbContext> options) : base(options)
        {
        }

        // Loan Management entities
        public DbSet<LoanProduct> LoanProducts { get; set; }
        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<LoanGuarantor> LoanGuarantors { get; set; }
        public DbSet<LoanCollateral> LoanCollaterals { get; set; }
        public DbSet<LoanCollateralDocument> LoanCollateralDocuments { get; set; }
        public DbSet<LoanDocument> LoanDocuments { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<LoanAccount> LoanAccounts { get; set; }
        public DbSet<LoanRepaymentSchedule> LoanRepaymentSchedules { get; set; }
        public DbSet<LoanTransaction> LoanTransactions { get; set; }
        public DbSet<LoanClassificationHistory> LoanClassificationHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Apply loan-specific configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(LoansDbContext).Assembly, 
                type => type.Namespace?.Contains("Loans") == true);
        }
    }
}
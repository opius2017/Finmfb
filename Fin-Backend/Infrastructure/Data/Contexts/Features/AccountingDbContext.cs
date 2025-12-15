using Microsoft.EntityFrameworkCore;
using FinTech.Core.Domain.Entities.Accounting;
using FinTech.Core.Domain.Entities.GeneralLedger;

namespace FinTech.Infrastructure.Data.Contexts.Features
{
    /// <summary>
    /// Database context for Accounting feature module
    /// </summary>
    public class AccountingDbContext : DbContext
    {
        public AccountingDbContext(DbContextOptions<AccountingDbContext> options) : base(options)
        {
        }

        // General Ledger entities
        public DbSet<ChartOfAccount> ChartOfAccounts { get; set; }
        public DbSet<FinTech.Core.Domain.Entities.Accounting.JournalEntry> JournalEntries { get; set; }
        public DbSet<JournalEntryLine> JournalEntryLines { get; set; }
        public DbSet<GeneralLedgerEntry> GeneralLedgerEntries { get; set; }
        // public DbSet<TrialBalance> TrialBalances { get; set; }
        public DbSet<FinancialPeriod> FinancialPeriods { get; set; }
        public DbSet<FiscalYear> FiscalYears { get; set; }
        
        // Accounts Payable/Receivable
        // public DbSet<AccountsPayable> AccountsPayables { get; set; }
        // public DbSet<AccountsReceivable> AccountsReceivables { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<VendorCategory> VendorCategories { get; set; }
        
        // Financial Reporting
        // public DbSet<FinancialReport> FinancialReports { get; set; }
        // public DbSet<FinancialReportTemplate> FinancialReportTemplates { get; set; }
        // public DbSet<ReportSchedule> ReportSchedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Apply accounting-specific configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountingDbContext).Assembly, 
                type => type.Namespace?.Contains("Accounting") == true);
        }
    }
}
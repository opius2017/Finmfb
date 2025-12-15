using FinTech.Core.Domain.Entities.Loans;
using FinTech.Core.Domain.Entities.Tax;
using Microsoft.EntityFrameworkCore;

namespace FinTech.Infrastructure.Persistence
{
    /// <summary>
    /// Application database context
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        
        /// <summary>
        /// Tax types DbSet
        /// </summary>
        public DbSet<TaxType> TaxTypes { get; set; }
        
        /// <summary>
        /// Tax rates DbSet
        /// </summary>
        public DbSet<TaxRate> TaxRates { get; set; }
        
        /// <summary>
        /// Tax transactions DbSet
        /// </summary>
        public DbSet<TaxTransaction> TaxTransactions { get; set; }
        
        /// <summary>
        /// Tax exemptions DbSet
        /// </summary>
        public DbSet<TaxExemption> TaxExemptions { get; set; }
        
        /// <summary>
        /// Loan entities DbSets
        /// </summary>
        public DbSet<Loan> Loans { get; set; }
        public DbSet<LoanProduct> LoanProducts { get; set; }
        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<LoanTransaction> LoanTransactions { get; set; }
        public DbSet<LoanRepaymentSchedule> LoanRepaymentSchedules { get; set; }
        public DbSet<LoanDocument> LoanDocuments { get; set; }
        public DbSet<LoanCollateral> LoanCollaterals { get; set; }
        public DbSet<LoanGuarantor> LoanGuarantors { get; set; }
        public DbSet<LoanCollection> LoanCollections { get; set; }
        public DbSet<LoanFee> LoanFees { get; set; }
        public DbSet<LoanCreditCheck> LoanCreditChecks { get; set; }
        
        /// <summary>
        /// Configure model relationships and constraints
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Apply configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}

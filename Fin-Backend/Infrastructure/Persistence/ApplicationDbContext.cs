using FinTech.Domain.Entities.Tax;
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
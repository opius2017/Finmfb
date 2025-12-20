using FinTech.Core.Domain.Entities.ClientPortal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations.ClientPortal
{
    public class BillPaymentConfiguration : IEntityTypeConfiguration<BillPayment>
    {
        public void Configure(EntityTypeBuilder<BillPayment> builder)
        {
            builder.ToTable("BillPayments");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Amount)
                .HasColumnType("decimal(18,2)");

            // Relationships
            
            // Break cascade cycle: Customer -> DepositAccount -> BillPayment VS Customer -> BillPayment
            // Determine which path to restrict.
            // If Customer is deleted, typically everything is deleted.
            // But if Account is deleted, BillPayments should probably be kept (history) or deleted.
            // Restricting Account -> BillPayment makes sense if we want to prevent accidental account deletion if payments exist.
            // OR we can make Customer -> BillPayment Restrict if the cascade comes via Account.
            
            // Let's restrict Account relationship.
            builder.HasOne(e => e.Account)
                .WithMany() // Assuming no navigation back, or generic
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict); 

            // Allow Customer cascade
            builder.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasOne(e => e.Biller)
                .WithMany(b => b.BillPayments)
                .HasForeignKey(e => e.BillerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

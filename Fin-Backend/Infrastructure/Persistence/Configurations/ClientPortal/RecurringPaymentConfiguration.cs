using FinTech.Core.Domain.Entities.ClientPortal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations.ClientPortal
{
    public class RecurringPaymentConfiguration : IEntityTypeConfiguration<RecurringPayment>
    {
        public void Configure(EntityTypeBuilder<RecurringPayment> builder)
        {
            builder.ToTable("RecurringPayments");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Amount)
                .HasColumnType("decimal(18,2)");

            // Relationships
            builder.HasOne(e => e.SourceAccount)
                .WithMany()
                .HasForeignKey(e => e.SourceAccountId)
                .OnDelete(DeleteBehavior.Restrict); // Fix multiple cascade paths

            builder.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Biller)
                .WithMany() // Assuming no collection back
                .HasForeignKey(e => e.BillerId)
                .OnDelete(DeleteBehavior.SetNull); // Or Restrict. SetNull if Biller deleted but we keep history? Or Cascade? Let's use SetNull or Restrict.
        }
    }
}

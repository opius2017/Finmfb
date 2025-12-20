using FinTech.Core.Domain.Entities.ClientPortal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations.ClientPortal
{
    public class ExternalTransferConfiguration : IEntityTypeConfiguration<ExternalTransfer>
    {
        public void Configure(EntityTypeBuilder<ExternalTransfer> builder)
        {
            builder.ToTable("ExternalTransfers");

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
        }
    }
}

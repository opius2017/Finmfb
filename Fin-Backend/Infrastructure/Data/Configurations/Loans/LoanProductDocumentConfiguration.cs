using FinTech.Core.Domain.Entities.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Data.Configurations.Loans
{
    public class LoanProductDocumentConfiguration : IEntityTypeConfiguration<LoanProductDocument>
    {
        public void Configure(EntityTypeBuilder<LoanProductDocument> builder)
        {
            builder.ToTable("LoanProductDocuments", "loans");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.DocumentType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(d => d.DocumentName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.Description)
                .HasMaxLength(500);

            builder.Property(d => d.ApplicableFor)
                .HasMaxLength(50);

            builder.HasOne(d => d.LoanProduct)
                .WithMany(p => p.RequiredDocuments)
                .HasForeignKey(d => d.LoanProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

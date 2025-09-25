using FinTech.Core.Domain.Entities.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class LoanCollectionConfiguration : IEntityTypeConfiguration<LoanCollection>
    {
        public void Configure(EntityTypeBuilder<LoanCollection> builder)
        {
            builder.ToTable("LoanCollections", "loans");

            builder.HasKey(lc => lc.Id);
            
            builder.Property(lc => lc.LoanId)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(lc => lc.CustomerId)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(lc => lc.AmountDue)
                .HasPrecision(18, 2)
                .IsRequired();
                
            builder.Property(lc => lc.AmountCollected)
                .HasPrecision(18, 2)
                .IsRequired();
                
            builder.Property(lc => lc.Status)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(lc => lc.CollectionMethod)
                .HasMaxLength(100);
                
            builder.Property(lc => lc.CollectionAgentId)
                .HasMaxLength(100);
                
            builder.Property(lc => lc.CollectionNotes)
                .HasMaxLength(500);
                
            builder.Property(lc => lc.NextFollowUpDate);
                
            builder.Property(lc => lc.FollowUpAction)
                .HasMaxLength(500);
                
            builder.HasIndex(lc => lc.LoanId);
            builder.HasIndex(lc => lc.CustomerId);
            builder.HasIndex(lc => lc.DueDate);
            builder.HasIndex(lc => lc.Status);
            builder.HasIndex(lc => lc.CollectionAgentId);
            
            // Relationships
            builder.HasOne<Loan>()
                .WithMany()
                .HasForeignKey(lc => lc.LoanId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

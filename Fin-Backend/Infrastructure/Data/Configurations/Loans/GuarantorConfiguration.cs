using FinTech.Core.Domain.Entities.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Data.Configurations.Loans
{
    public class GuarantorConfiguration : IEntityTypeConfiguration<Guarantor>
    {
        public void Configure(EntityTypeBuilder<Guarantor> builder)
        {
            builder.ToTable("Guarantors");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.GuaranteeAmount)
                .HasColumnType("decimal(18,2)");
                
            builder.Property(g => g.LockedEquity)
                .HasColumnType("decimal(18,2)");

            builder.Property(g => g.CurrentExposure)
                .HasColumnType("decimal(18,2)");

            builder.Property(g => g.MaximumExposure)
                .HasColumnType("decimal(18,2)");

            // Relationship for GuarantorMember (Provider of guarantee)
            builder.HasOne(g => g.GuarantorMember)
                .WithMany(m => m.GuarantorsProvided)
                .HasForeignKey(g => g.GuarantorMemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship for Borrower (MemberId)
            builder.HasOne<Member>() 
                .WithMany(m => m.GuarantorsReceived)
                .HasForeignKey(g => g.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

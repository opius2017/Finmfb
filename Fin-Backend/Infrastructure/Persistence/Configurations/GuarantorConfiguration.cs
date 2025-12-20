using FinTech.Core.Domain.Entities.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class GuarantorConfiguration : IEntityTypeConfiguration<Guarantor>
    {
        public void Configure(EntityTypeBuilder<Guarantor> builder)
        {
            builder.ToTable("Guarantors", "loans");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.GuarantorMemberId)
                .IsRequired(); // Inherited from FK

            // Use Restrict to prevent cycles with Member
            builder.HasOne(g => g.GuarantorMember)
                .WithMany() // Assuming Member has collection of Guarantors? If not, WithMany() is fine for uni-directional
                .HasForeignKey(g => g.GuarantorMemberId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(g => g.LoanApplication)
                .WithMany(la => la.Guarantors)
                .HasForeignKey(g => g.LoanApplicationId)
                .OnDelete(DeleteBehavior.Cascade); // Guarantor belongs to Application usually

            builder.HasOne(g => g.Loan)
                .WithMany() // Loan doesn't seem to have Guarantors collection in snippet? Wait, logic check.
                .HasForeignKey(g => g.LoanId)
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}

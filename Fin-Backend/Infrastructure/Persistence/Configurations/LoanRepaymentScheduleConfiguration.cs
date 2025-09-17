using FinTech.Domain.Entities.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class LoanRepaymentScheduleConfiguration : IEntityTypeConfiguration<LoanRepaymentSchedule>
    {
        public void Configure(EntityTypeBuilder<LoanRepaymentSchedule> builder)
        {
            builder.ToTable("LoanRepaymentSchedules", "loans");

            builder.HasKey(lrs => lrs.Id);
            
            builder.Property(lrs => lrs.LoanId)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(lrs => lrs.InstallmentNumber)
                .IsRequired();
                
            builder.Property(lrs => lrs.PrincipalAmount)
                .HasPrecision(18, 2)
                .IsRequired();
                
            builder.Property(lrs => lrs.InterestAmount)
                .HasPrecision(18, 2)
                .IsRequired();
                
            builder.Property(lrs => lrs.TotalAmount)
                .HasPrecision(18, 2)
                .IsRequired();
                
            builder.Property(lrs => lrs.PaidAmount)
                .HasPrecision(18, 2)
                .IsRequired();
                
            builder.Property(lrs => lrs.Status)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.HasIndex(lrs => lrs.LoanId);
            builder.HasIndex(lrs => lrs.DueDate);
            builder.HasIndex(lrs => lrs.Status);
            
            // Relationships
            builder.HasOne<Loan>()
                .WithMany()
                .HasForeignKey(lrs => lrs.LoanId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
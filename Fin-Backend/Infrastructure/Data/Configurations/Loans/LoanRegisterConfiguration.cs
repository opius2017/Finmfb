using FinTech.Core.Domain.Entities.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Data.Configurations.Loans
{
    public class LoanRegisterConfiguration : IEntityTypeConfiguration<LoanRegister>
    {
        public void Configure(EntityTypeBuilder<LoanRegister> builder)
        {
            builder.ToTable("LoanRegisters");
            
            builder.HasKey(lr => lr.Id);
            
            builder.Property(lr => lr.SerialNumber)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.HasIndex(lr => lr.SerialNumber)
                .IsUnique();
            
            builder.Property(lr => lr.MemberNumber)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(lr => lr.MemberName)
                .IsRequired()
                .HasMaxLength(200);
            
            builder.Property(lr => lr.PrincipalAmount)
                .HasColumnType("decimal(18,2)");
            
            builder.Property(lr => lr.InterestRate)
                .HasColumnType("decimal(5,2)");
            
            builder.Property(lr => lr.MonthlyEMI)
                .HasColumnType("decimal(18,2)");
            
            builder.Property(lr => lr.LoanType)
                .HasMaxLength(50);
            
            builder.Property(lr => lr.RegisteredBy)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(lr => lr.Status)
                .HasMaxLength(20);
            
            builder.Property(lr => lr.Notes)
                .HasMaxLength(1000);
            
            // Relationships
            builder.HasOne(lr => lr.Loan)
                .WithMany()
                .HasForeignKey(lr => lr.LoanId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(lr => lr.Application)
                .WithMany()
                .HasForeignKey(lr => lr.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(lr => lr.Member)
                .WithMany()
                .HasForeignKey(lr => lr.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes
            builder.HasIndex(lr => lr.LoanId);
            builder.HasIndex(lr => lr.ApplicationId);
            builder.HasIndex(lr => lr.MemberId);
            builder.HasIndex(lr => new { lr.RegistrationYear, lr.RegistrationMonth });
            builder.HasIndex(lr => lr.SequenceNumber);
            builder.HasIndex(lr => lr.RegistrationDate);
            builder.HasIndex(lr => lr.Status);
        }
    }
}

using FinTech.Core.Domain.Entities.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Data.Configurations.Loans
{
    public class MemberConfiguration : IEntityTypeConfiguration<Member>
    {
        public void Configure(EntityTypeBuilder<Member> builder)
        {
            builder.ToTable("Members");
            
            builder.HasKey(m => m.Id);
            
            builder.Property(m => m.MemberNumber)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.HasIndex(m => m.MemberNumber)
                .IsUnique();
            
            builder.Property(m => m.FirstName)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(m => m.LastName)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(m => m.PayrollPin)
                .HasMaxLength(50);
            
            builder.Property(m => m.TotalSavings)
                .HasColumnType("decimal(18,2)");
            
            builder.Property(m => m.MonthlyContribution)
                .HasColumnType("decimal(18,2)");
            
            builder.Property(m => m.ShareCapital)
                .HasColumnType("decimal(18,2)");
            
            builder.Property(m => m.FreeEquity)
                .HasColumnType("decimal(18,2)");
            
            builder.Property(m => m.LockedEquity)
                .HasColumnType("decimal(18,2)");
            
            builder.Property(m => m.GrossSalary)
                .HasColumnType("decimal(18,2)");
            
            builder.Property(m => m.NetSalary)
                .HasColumnType("decimal(18,2)");
            
            builder.Property(m => m.StatutoryDeductions)
                .HasColumnType("decimal(18,2)");
            
            builder.Property(m => m.CreditScore)
                .HasColumnType("decimal(5,2)");
            
            builder.Property(m => m.TotalOutstandingLoans)
                .HasColumnType("decimal(18,2)");
            
            builder.Property(m => m.RiskRating)
                .HasMaxLength(20);
            
            builder.Property(m => m.Status)
                .HasMaxLength(20);
            
            // Relationships
            builder.HasMany(m => m.LoanApplications)
                .WithOne()
                .HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasMany(m => m.Loans)
                .WithOne()
                .HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Restrict);
            
            /*
            builder.HasMany(m => m.GuarantorObligations)
                .WithOne()
                .HasForeignKey("MemberId")
                .OnDelete(DeleteBehavior.Restrict);
            */
            
            // Indexes
            builder.HasIndex(m => m.Email);
            builder.HasIndex(m => m.PhoneNumber);
            builder.HasIndex(m => m.PayrollPin);
            builder.HasIndex(m => m.IsActive);
            builder.HasIndex(m => m.MembershipDate);
        }
    }
}

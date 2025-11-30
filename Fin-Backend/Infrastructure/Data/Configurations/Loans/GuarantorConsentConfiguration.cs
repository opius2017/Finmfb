using FinTech.Core.Domain.Entities.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Data.Configurations.Loans
{
    public class GuarantorConsentConfiguration : IEntityTypeConfiguration<GuarantorConsent>
    {
        public void Configure(EntityTypeBuilder<GuarantorConsent> builder)
        {
            builder.ToTable("GuarantorConsents");
            
            builder.HasKey(gc => gc.Id);
            
            builder.Property(gc => gc.ConsentToken)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.HasIndex(gc => gc.ConsentToken)
                .IsUnique();
            
            builder.Property(gc => gc.GuaranteedAmount)
                .HasColumnType("decimal(18,2)");
            
            builder.Property(gc => gc.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);
            
            builder.Property(gc => gc.DeclineReason)
                .HasMaxLength(500);
            
            builder.Property(gc => gc.Notes)
                .HasMaxLength(1000);
            
            // Relationships
            builder.HasOne(gc => gc.Application)
                .WithMany()
                .HasForeignKey(gc => gc.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(gc => gc.GuarantorMember)
                .WithMany()
                .HasForeignKey(gc => gc.GuarantorMemberId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(gc => gc.ApplicantMember)
                .WithMany()
                .HasForeignKey(gc => gc.ApplicantMemberId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes
            builder.HasIndex(gc => gc.ApplicationId);
            builder.HasIndex(gc => gc.GuarantorMemberId);
            builder.HasIndex(gc => gc.Status);
            builder.HasIndex(gc => gc.RequestedAt);
            builder.HasIndex(gc => gc.ExpiresAt);
        }
    }
}

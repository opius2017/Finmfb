using FinTech.Core.Domain.Entities.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Data.Configurations.Loans
{
    public class CommitteeReviewConfiguration : IEntityTypeConfiguration<CommitteeReview>
    {
        public void Configure(EntityTypeBuilder<CommitteeReview> builder)
        {
            builder.ToTable("CommitteeReviews");
            
            builder.HasKey(cr => cr.Id);
            
            /*
            builder.Property(cr => cr.ReviewerUserId)
                .IsRequired()
                .HasMaxLength(100);
            */
            
            builder.Property(cr => cr.ReviewerName)
                .IsRequired()
                .HasMaxLength(200);
            
            builder.Property(cr => cr.Decision)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(30);
            
            builder.Property(cr => cr.Comments)
                .HasMaxLength(2000);
            
            /*
            builder.Property(cr => cr.RecommendedAction)
                .HasMaxLength(500);
            */
            
            /*
            builder.Property(cr => cr.CreditScore)
                .HasColumnType("decimal(5,2)");
            */
            
            /*
            builder.Property(cr => cr.RiskRating)
                .HasMaxLength(20);
            */
            
            /*
            builder.Property(cr => cr.RepaymentScore)
                .HasColumnType("decimal(5,2)");
            */
            
            builder.Property(cr => cr.RecommendedAmount)
                .HasColumnType("decimal(18,2)");
            
            builder.Property(cr => cr.RecommendedInterestRate)
                .HasColumnType("decimal(5,2)");
            
            /*
            // Relationships
            builder.HasOne(cr => cr.Application)
                .WithMany()
                .HasForeignKey(cr => cr.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);
            */
            
            // Indexes
            /*
            builder.HasIndex(cr => cr.ApplicationId);
            builder.HasIndex(cr => cr.ReviewerUserId);
            */
            builder.HasIndex(cr => cr.Decision);
            builder.HasIndex(cr => cr.ReviewDate);
        }
    }
}

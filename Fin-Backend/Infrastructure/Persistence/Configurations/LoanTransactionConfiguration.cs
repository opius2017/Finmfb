using FinTech.Core.Domain.Entities.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Persistence.Configurations
{
    public class LoanTransactionConfiguration : IEntityTypeConfiguration<LoanTransaction>
    {
        public void Configure(EntityTypeBuilder<LoanTransaction> builder)
        {
            builder.ToTable("LoanTransactions", "loans");

            builder.HasKey(lt => lt.Id);
            
            builder.Property(lt => lt.LoanId)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(lt => lt.TransactionType)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(lt => lt.Amount)
                .HasPrecision(18, 2)
                .IsRequired();
                
            builder.Property(lt => lt.ReferenceNumber)
                .HasMaxLength(100);
                
            builder.Property(lt => lt.Description)
                .HasMaxLength(500);
                
            builder.Property(lt => lt.ProcessedBy)
                .HasMaxLength(100);
                
            builder.Property(lt => lt.PaymentMethod)
                .HasMaxLength(50);
                
            builder.Property(lt => lt.Status)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.HasIndex(lt => lt.LoanId);
            builder.HasIndex(lt => lt.TransactionDate);
            builder.HasIndex(lt => lt.ReferenceNumber);
            
            // Relationships
            builder.HasOne<Loan>()
                .WithMany()
                .HasForeignKey(lt => lt.LoanId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

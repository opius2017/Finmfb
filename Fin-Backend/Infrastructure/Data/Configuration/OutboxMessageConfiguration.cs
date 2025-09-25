using FinTech.Infrastructure.Data.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Data.Configuration
{
    /// <summary>
    /// Entity Framework configuration for the OutboxMessage entity
    /// </summary>
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(512);
                
            builder.Property(e => e.Content)
                .IsRequired()
                .HasColumnType("nvarchar(max)");
                
            builder.Property(e => e.Error)
                .HasColumnType("nvarchar(max)");
                
            // Create indexes for better query performance
            builder.HasIndex(e => e.CreatedAt);
            builder.HasIndex(e => e.ProcessedAt);
            builder.HasIndex(e => new { e.EventType, e.ProcessedAt });
        }
    }
}

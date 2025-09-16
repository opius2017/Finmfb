using FinTech.Infrastructure.Data.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Data.Configuration
{
    /// <summary>
    /// Entity Framework configuration for the DomainEventRecord entity
    /// </summary>
    public class DomainEventRecordConfiguration : IEntityTypeConfiguration<DomainEventRecord>
    {
        public void Configure(EntityTypeBuilder<DomainEventRecord> builder)
        {
            builder.ToTable("DomainEventRecords");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(256);
                
            builder.Property(e => e.EntityName)
                .IsRequired()
                .HasMaxLength(256);
                
            builder.Property(e => e.EntityId)
                .IsRequired()
                .HasMaxLength(256);
                
            builder.Property(e => e.Data)
                .HasColumnType("nvarchar(max)");
                
            // Create indexes for better query performance
            builder.HasIndex(e => e.EventType);
            builder.HasIndex(e => e.EntityName);
            builder.HasIndex(e => e.EntityId);
            builder.HasIndex(e => e.CreatedAt);
        }
    }
}
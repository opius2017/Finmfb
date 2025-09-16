using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Data.Configurations.Accounting
{
    public class DomainEventRecordConfiguration : IEntityTypeConfiguration<DomainEventRecord>
    {
        public void Configure(EntityTypeBuilder<DomainEventRecord> builder)
        {
            builder.ToTable("DomainEventRecords");
            
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.EventType)
                .IsRequired()
                .HasMaxLength(256);
                
            builder.Property(x => x.EntityName)
                .IsRequired()
                .HasMaxLength(128);
                
            builder.Property(x => x.EntityId)
                .IsRequired()
                .HasMaxLength(128);
                
            builder.Property(x => x.CreatedAt)
                .IsRequired();
                
            builder.Property(x => x.Data)
                .IsRequired()
                .HasColumnType("nvarchar(max)");
                
            // Create indexes for efficient querying
            builder.HasIndex(x => x.EventType);
            builder.HasIndex(x => new { x.EntityName, x.EntityId });
            builder.HasIndex(x => x.CreatedAt);
        }
    }
}
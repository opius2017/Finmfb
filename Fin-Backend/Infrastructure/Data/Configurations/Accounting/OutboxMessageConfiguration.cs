using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FinTech.Core.Domain.Entities.Common;

namespace FinTech.Infrastructure.Data.Configurations.Accounting
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages");
            
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.EventType)
                .IsRequired()
                .HasMaxLength(500);
                
            builder.Property(x => x.Content)
                .IsRequired()
                .HasColumnType("nvarchar(max)");
                
            builder.Property(x => x.CreatedAt)
                .IsRequired();
                
            builder.Property(x => x.ProcessedAt)
                .IsRequired(false);
                
            builder.Property(x => x.Error)
                .IsRequired(false)
                .HasColumnType("nvarchar(max)");
                
            // Create indexes for efficient querying
            builder.HasIndex(x => x.EventType);
            builder.HasIndex(x => x.ProcessedAt);
            builder.HasIndex(x => x.CreatedAt);
        }
    }
}

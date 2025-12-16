using FinTech.Core.Domain.Entities.ClientPortal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Data.Configurations.ClientPortal
{
    public class ClientPortalProfileConfiguration : IEntityTypeConfiguration<ClientPortalProfile>
    {
        public void Configure(EntityTypeBuilder<ClientPortalProfile> builder)
        {
            builder.HasKey(p => p.Id);
            
            /*
            builder.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            */
                
            builder.HasOne(p => p.Customer)
                .WithMany()
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            
            /*
            builder.Property(p => p.PreferredLanguage)
                .IsRequired()
                .HasMaxLength(10);
            */
                
            /*
            builder.Property(p => p.TimeZone)
                .IsRequired()
                .HasMaxLength(50);
            */
                
            /*
            builder.Property(p => p.PushNotificationToken)
                .HasMaxLength(255);
            */
        }
    }
}
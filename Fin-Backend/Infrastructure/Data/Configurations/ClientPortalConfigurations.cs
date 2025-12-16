using FinTech.Core.Domain.Entities.ClientPortal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTech.Infrastructure.Data.Configurations
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
                
            builder.HasOne(p => p.Customer)
                .WithMany()
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Property(p => p.PreferredLanguage)
                .IsRequired()
                .HasMaxLength(10);
                
            builder.Property(p => p.TimeZone)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(p => p.PushNotificationToken)
                .HasMaxLength(255);
            */
        }
    }
    
    public class NotificationPreferencesConfiguration : IEntityTypeConfiguration<NotificationPreferences>
    {
        public void Configure(EntityTypeBuilder<NotificationPreferences> builder)
        {
            builder.HasKey(p => p.Id);
            
            /*
            builder.HasOne(p => p.ClientPortalProfile)
                .WithOne(c => c.NotificationPreferences)
                .HasForeignKey<NotificationPreferences>(p => p.ClientPortalProfileId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Property(p => p.TransactionAlertThreshold)
                .HasColumnType("decimal(18, 2)");
            */
                
            builder.Property(p => p.LowBalanceThreshold)
                .HasColumnType("decimal(18, 2)");
        }
    }
    
    public class DashboardPreferencesConfiguration : IEntityTypeConfiguration<DashboardPreferences>
    {
        public void Configure(EntityTypeBuilder<DashboardPreferences> builder)
        {
            builder.HasKey(p => p.Id);
            
            /*
            builder.HasOne(p => p.ClientPortalProfile)
                .WithOne(c => c.DashboardPreferences)
                .HasForeignKey<DashboardPreferences>(p => p.ClientPortalProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            */
                
            builder.Property(p => p.Layout)
                .IsRequired()
                .HasMaxLength(50);
                
            /*
            // Store arrays as JSON
            builder.Property(p => p.VisibleWidgets)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
                    
            builder.Property(p => p.WidgetOrder)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
            */
        }
    }
    
    public class ClientSessionConfiguration : IEntityTypeConfiguration<ClientSession>
    {
        public void Configure(EntityTypeBuilder<ClientSession> builder)
        {
            builder.HasKey(s => s.Id);
            
            builder.HasOne(s => s.ClientPortalProfile)
                .WithMany(p => p.Sessions)
                .HasForeignKey(s => s.ClientPortalProfileId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Property(s => s.SessionId)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(s => s.IpAddress)
                .HasMaxLength(50);
                
            builder.Property(s => s.UserAgent)
                .HasMaxLength(500);
                
            builder.Property(s => s.DeviceType)
                .HasMaxLength(50);
                
            builder.Property(s => s.Browser)
                .HasMaxLength(100);
                
            builder.Property(s => s.OperatingSystem)
                .HasMaxLength(100);
                
            builder.Property(s => s.Location)
                .HasMaxLength(255);
                
            builder.Property(s => s.FailureReason)
                .HasMaxLength(255);
        }
    }
    
    public class ClientPortalActivityConfiguration : IEntityTypeConfiguration<ClientPortalActivity>
    {
        public void Configure(EntityTypeBuilder<ClientPortalActivity> builder)
        {
            builder.HasKey(a => a.Id);
            
            builder.HasOne(a => a.ClientPortalSession)
                .WithMany(s => s.Activities)
                .HasForeignKey(a => a.ClientPortalSessionId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Property(a => a.ActivityType)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(a => a.Page)
                .HasMaxLength(255);
                
            builder.Property(a => a.Action)
                .HasMaxLength(100);
                
            builder.Property(a => a.Details)
                .HasMaxLength(1000);
        }
    }
    
    public class SavedPayeeConfiguration : IEntityTypeConfiguration<SavedPayee>
    {
        public void Configure(EntityTypeBuilder<SavedPayee> builder)
        {
            builder.HasKey(p => p.Id);
            
            builder.HasOne(p => p.ClientPortalProfile)
                .WithMany(c => c.SavedPayees)
                .HasForeignKey(p => p.ClientPortalProfileId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Property(p => p.PayeeName)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(p => p.AccountNumber)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(p => p.BankName)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(p => p.BankCode)
                .IsRequired()
                .HasMaxLength(20);
                
            builder.Property(p => p.PayeeType)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(p => p.Reference)
                .HasMaxLength(100);
                
            builder.Property(p => p.Category)
                .HasMaxLength(50);
        }
    }
    
    public class SavedTransferTemplateConfiguration : IEntityTypeConfiguration<SavedTransferTemplate>
    {
        public void Configure(EntityTypeBuilder<SavedTransferTemplate> builder)
        {
            builder.HasKey(t => t.Id);
            
            builder.HasOne(t => t.ClientPortalProfile)
                .WithMany(c => c.SavedTransferTemplates)
                .HasForeignKey(t => t.ClientPortalProfileId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Property(t => t.TemplateName)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(t => t.FromAccountNumber)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(t => t.ToAccountNumber)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(t => t.ToBankName)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(t => t.ToBankCode)
                .IsRequired()
                .HasMaxLength(20);
                
            builder.Property(t => t.Amount)
                .HasColumnType("decimal(18, 2)");
                
            builder.Property(t => t.Currency)
                .IsRequired()
                .HasMaxLength(3);
                
            builder.Property(t => t.Reference)
                .HasMaxLength(100);
                
            builder.Property(t => t.Category)
                .HasMaxLength(50);
        }
    }
    
    public class ClientDocumentConfiguration : IEntityTypeConfiguration<ClientDocument>
    {
        public void Configure(EntityTypeBuilder<ClientDocument> builder)
        {
            builder.HasKey(d => d.Id);
            
            builder.HasOne(d => d.ClientPortalProfile)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.ClientPortalProfileId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Property(d => d.DocumentType)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(d => d.DocumentName)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.Property(d => d.FileName)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.Property(d => d.MimeType)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(d => d.FilePath)
                .IsRequired()
                .HasMaxLength(1000);
                
            builder.Property(d => d.StorageProvider)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(d => d.StorageReference)
                .HasMaxLength(1000);
        }
    }
    
    public class ClientSupportTicketConfiguration : IEntityTypeConfiguration<ClientSupportTicket>
    {
        public void Configure(EntityTypeBuilder<ClientSupportTicket> builder)
        {
            builder.HasKey(t => t.Id);
            
            /*
            builder.HasOne(t => t.ClientPortalProfile)
                .WithMany(c => c.SupportTickets)
                .HasForeignKey(t => t.ClientPortalProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            */
                
            builder.Property(t => t.TicketNumber)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(t => t.Subject)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.Property(t => t.Description)
                .IsRequired()
                .HasMaxLength(2000);
                
            builder.Property(t => t.Category)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(t => t.Priority)
                .IsRequired()
                .HasMaxLength(20);
                
            builder.Property(t => t.Status)
                .IsRequired()
                .HasMaxLength(20);
                
            builder.Property(t => t.Resolution)
                .HasMaxLength(2000);
        }
    }
    
    public class ClientSupportMessageConfiguration : IEntityTypeConfiguration<ClientSupportMessage>
    {
        public void Configure(EntityTypeBuilder<ClientSupportMessage> builder)
        {
            builder.HasKey(m => m.Id);
            
            /*
            builder.HasOne(m => m.ClientSupportTicket)
                .WithMany(t => t.Messages)
                .HasForeignKey(m => m.ClientSupportTicketId)
                .OnDelete(DeleteBehavior.Cascade);
            */
                
            builder.Property(m => m.Message)
                .IsRequired()
                .HasMaxLength(2000);
                
            builder.Property(m => m.SenderName)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
    
    public class ClientSupportAttachmentConfiguration : IEntityTypeConfiguration<ClientSupportAttachment>
    {
        public void Configure(EntityTypeBuilder<ClientSupportAttachment> builder)
        {
            builder.HasKey(a => a.Id);
            
            /*
            builder.HasOne(a => a.ClientSupportTicket)
                .WithMany(t => t.Attachments)
                .HasForeignKey(a => a.ClientSupportTicketId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Property(a => a.FileName)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.Property(a => a.MimeType)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(a => a.FilePath)
                .IsRequired()
                .HasMaxLength(1000);
            */
        }
    }
    
    public class SavingsGoalConfiguration : IEntityTypeConfiguration<SavingsGoal>
    {
        public void Configure(EntityTypeBuilder<SavingsGoal> builder)
        {
            builder.HasKey(g => g.Id);
            
            /*
            builder.HasOne(g => g.ClientPortalProfile)
                .WithMany(c => c.SavingsGoals)
                .HasForeignKey(g => g.ClientPortalProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            */
                
            builder.Property(g => g.GoalName)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(g => g.Description)
                .HasMaxLength(500);
                
            builder.Property(g => g.TargetAmount)
                .HasColumnType("decimal(18, 2)");
                
            builder.Property(g => g.CurrentAmount)
                .HasColumnType("decimal(18, 2)");
                
            /*
            builder.Property(g => g.Currency)
                .IsRequired()
                .HasMaxLength(3);
                
            builder.Property(g => g.GoalCategory)
                .HasMaxLength(50);
                
            builder.Property(g => g.Status)
                .IsRequired()
                .HasMaxLength(20);
                
            builder.Property(g => g.RecurrencePattern)
                .HasMaxLength(20);
                
            builder.Property(g => g.AutoTransferAmount)
                .HasColumnType("decimal(18, 2)");
                
            builder.Property(g => g.SourceAccountNumber)
                .HasMaxLength(50);
                
            builder.Property(g => g.DestinationAccountNumber)
                .HasMaxLength(50);
            */
        }
    }
    
    public class SavingsGoalTransactionConfiguration : IEntityTypeConfiguration<SavingsGoalTransaction>
    {
        public void Configure(EntityTypeBuilder<SavingsGoalTransaction> builder)
        {
            builder.HasKey(t => t.Id);
            
            builder.HasOne(t => t.SavingsGoal)
                .WithMany(g => g.Transactions)
                .HasForeignKey(t => t.SavingsGoalId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Property(t => t.Amount)
                .HasColumnType("decimal(18, 2)");
                
            /*
            builder.Property(t => t.Currency)
                .IsRequired()
                .HasMaxLength(3);
            */
                
            builder.Property(t => t.TransactionType)
                .IsRequired()
                .HasMaxLength(20);
                
            /*
            builder.Property(t => t.SourceAccountNumber)
                .HasMaxLength(50);
                
            builder.Property(t => t.DestinationAccountNumber)
                .HasMaxLength(50);
            */
                
            builder.Property(t => t.Reference)
                .HasMaxLength(100);
                
            builder.Property(t => t.Status)
                .IsRequired()
                .HasMaxLength(20);
        }
    }
    
    public class ClientNotificationConfiguration : IEntityTypeConfiguration<ClientNotification>
    {
        public void Configure(EntityTypeBuilder<ClientNotification> builder)
        {
            builder.HasKey(n => n.Id);
            
            /*
            builder.HasOne(n => n.ClientPortalProfile)
                .WithMany()
                .HasForeignKey(n => n.ClientPortalProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            */
                
            builder.Property(n => n.NotificationType)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(255);
                
            builder.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(1000);
                
            builder.Property(n => n.Action)
                .HasMaxLength(1000);
                
            builder.Property(n => n.DeliveryChannels)
                .HasMaxLength(100);
                
            builder.Property(n => n.DeliveryStatus)
                .HasMaxLength(20);
        }
    }
}

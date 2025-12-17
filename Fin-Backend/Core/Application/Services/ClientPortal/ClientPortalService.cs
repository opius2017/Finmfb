using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Core.Domain.Entities.ClientPortal;
using FinTech.Core.Domain.Entities.Customers;
using FinTech.Core.Domain.Entities.Identity;
using FinTech.Core.Application.Common.Interfaces;

using FinTech.Core.Application.Common.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTech.Core.Application.Services.ClientPortal
{
    public interface IClientPortalService
    {
        Task<ClientPortalProfile> GetClientProfileAsync(Guid userId);
        Task<ClientPortalProfile> CreateClientProfileAsync(Guid userId, Guid customerId);
        Task<ClientPortalProfile> UpdateClientProfileAsync(ClientPortalProfile profile);
        Task<IEnumerable<ClientSession>> GetClientSessionsAsync(Guid profileId, int limit = 10);
        Task<ClientSession> StartSessionAsync(Guid profileId, string ipAddress, string userAgent);
        Task EndSessionAsync(Guid sessionId);

        Task<IEnumerable<ClientDocument>> GetClientDocumentsAsync(Guid profileId);
        Task<ClientDocument> UploadDocumentAsync(ClientDocument document);
        Task<IEnumerable<ClientSupportTicket>> GetSupportTicketsAsync(Guid profileId);
        Task<ClientSupportTicket> CreateSupportTicketAsync(ClientSupportTicket ticket);
        Task<ClientSupportTicket> UpdateSupportTicketAsync(ClientSupportTicket ticket);
        Task<ClientSupportMessage> AddSupportMessageAsync(ClientSupportMessage message);
        Task<IEnumerable<SavingsGoal>> GetSavingsGoalsAsync(Guid profileId);
        Task<SavingsGoal> CreateSavingsGoalAsync(SavingsGoal goal);
        Task<SavingsGoal> UpdateSavingsGoalAsync(SavingsGoal goal);
        Task<IEnumerable<SavedPayee>> GetSavedPayeesAsync(Guid profileId);
        Task<SavedPayee> CreateSavedPayeeAsync(SavedPayee payee);
        Task<SavedPayee> UpdateSavedPayeeAsync(SavedPayee payee);
        Task<bool> DeleteSavedPayeeAsync(Guid payeeId);
    }

    public class ClientPortalService : IClientPortalService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ILogger<ClientPortalService> _logger;

        public ClientPortalService(IApplicationDbContext dbContext, ILogger<ClientPortalService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<ClientPortalProfile> GetClientProfileAsync(Guid userId)
        {
            try
            {
                var profile = await _dbContext.ClientPortalProfiles
                    .Include(p => p.NotificationPreferences)
                    .Include(p => p.DashboardPreferences)
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                return profile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client profile for user {UserId}", userId);
                throw;
            }
        }

        public async Task<ClientPortalProfile> CreateClientProfileAsync(Guid userId, Guid customerId)
        {
            try
            {
                // Check if profile already exists
                var existingProfile = await _dbContext.ClientPortalProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (existingProfile != null)
                {
                    return existingProfile;
                }

                // Verify that user and customer exist
                var user = await _dbContext.Users.FindAsync(userId);
                var customer = await _dbContext.Customers.FindAsync(customerId);

                if (user == null || customer == null)
                {
                    throw new Exception($"User or customer not found. UserId: {userId}, CustomerId: {customerId}");
                }

                var profile = new ClientPortalProfile
                {
                    UserId = userId,
                    CustomerId = customerId,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    NotificationPreferences = new NotificationPreferences(),
                    DashboardPreferences = new DashboardPreferences()
                };

                _dbContext.ClientPortalProfiles.Add(profile);
                await _dbContext.SaveChangesAsync();

                return profile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client profile for user {UserId}", userId);
                throw;
            }
        }

        public async Task<ClientPortalProfile> UpdateClientProfileAsync(ClientPortalProfile profile)
        {
            try
            {
                profile.LastModifiedDate = DateTime.UtcNow;
                _dbContext.ClientPortalProfiles.Update(profile);
                await _dbContext.SaveChangesAsync();
                return profile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client profile {ProfileId}", profile.Id);
                throw;
            }
        }

        public async Task<IEnumerable<ClientSession>> GetClientSessionsAsync(Guid profileId, int limit = 10)
        {
            try
            {
                return await _dbContext.ClientSessions
                    .Where(s => s.ClientPortalProfileId == profileId.ToString())
                    .OrderByDescending(s => s.LoginTime)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client sessions for profile {ProfileId}", profileId);
                throw;
            }
        }


        public async Task<ClientSession> StartSessionAsync(Guid profileId, string ipAddress, string userAgent)
        {
            try
            {
                var deviceInfo = ParseUserAgent(userAgent);

                var session = new ClientSession
                {
                    ClientPortalProfileId = profileId.ToString(),
                    SessionId = Guid.NewGuid().ToString(),
                    LoginTime = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    DeviceType = deviceInfo.DeviceType,
                    Browser = deviceInfo.Browser,
                    OperatingSystem = deviceInfo.OperatingSystem,
                    IsActive = true,
                    IsSuccessful = true,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                };

                _dbContext.ClientSessions.Add(session);
                
                // Update the client profile with last login date
                var profile = await _dbContext.ClientPortalProfiles.FindAsync(profileId);
                if (profile != null)
                {
                    profile.LastLoginDate = DateTime.UtcNow;
                    profile.LoginCount++;
                    _dbContext.ClientPortalProfiles.Update(profile);
                }
                
                await _dbContext.SaveChangesAsync();
                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting client session for profile {ProfileId}", profileId);
                throw;
            }
        }


        public async Task EndSessionAsync(Guid sessionId)
        {
            try
            {
                // Note: sessionId passed here seems to be a Guid, but SessionId property is string. 
                // However, ClientSession probably has a PK Id (Guid).
                // Assuming sessionId is the PK.
                var session = await _dbContext.ClientSessions.FindAsync(sessionId);
                if (session != null)
                {
                    session.LogoutTime = DateTime.UtcNow;
                    session.IsActive = false;
                    session.LastModifiedDate = DateTime.UtcNow;
                    
                    _dbContext.ClientSessions.Update(session);
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending client session {SessionId}", sessionId);
                throw;
            }
        }


        public async Task<IEnumerable<ClientDocument>> GetClientDocumentsAsync(Guid profileId)
        {
            try
            {
                return await _dbContext.ClientDocuments
                    .Where(d => d.ClientPortalProfileId == profileId.ToString())
                    .OrderByDescending(d => d.CreatedOn)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client documents for profile {ProfileId}", profileId);
                throw;
            }
        }

        public async Task<ClientDocument> UploadDocumentAsync(ClientDocument document)
        {
            try
            {
                document.CreatedOn = DateTime.UtcNow;
                document.LastModifiedOn = DateTime.UtcNow;
                
                _dbContext.ClientDocuments.Add(document);
                await _dbContext.SaveChangesAsync();
                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document for profile {ProfileId}", document.ClientPortalProfileId);
                throw;
            }
        }

        public async Task<IEnumerable<ClientSupportTicket>> GetSupportTicketsAsync(Guid profileId)
        {
            try
            {
                return await _dbContext.ClientSupportTickets
                    .Where(t => t.ClientPortalProfileId == profileId.ToString())
                    .Include(t => t.Messages)
                    .OrderByDescending(t => t.CreatedOn)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving support tickets for profile {ProfileId}", profileId);
                throw;
            }
        }

        public async Task<ClientSupportTicket> CreateSupportTicketAsync(ClientSupportTicket ticket)
        {
            try
            {
                // Generate ticket number
                ticket.TicketNumber = GenerateTicketNumber();
                ticket.Status = "open";
                ticket.CreatedOn = DateTime.UtcNow;
                ticket.LastModifiedOn = DateTime.UtcNow;
                
                _dbContext.ClientSupportTickets.Add(ticket);
                await _dbContext.SaveChangesAsync();
                return ticket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating support ticket for profile {ProfileId}", ticket.ClientPortalProfileId);
                throw;
            }
        }

        public async Task<ClientSupportTicket> UpdateSupportTicketAsync(ClientSupportTicket ticket)
        {
            try
            {
                ticket.LastModifiedDate = DateTime.UtcNow;
                
                if (ticket.Status == "closed" && !ticket.ClosedDate.HasValue)
                {
                    ticket.ClosedDate = DateTime.UtcNow;
                }
                
                _dbContext.ClientSupportTickets.Update(ticket);
                await _dbContext.SaveChangesAsync();
                return ticket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating support ticket {TicketId}", ticket.Id);
                throw;
            }
        }

        public async Task<ClientSupportMessage> AddSupportMessageAsync(ClientSupportMessage message)
        {
            try
            {
                message.CreatedOn = DateTime.UtcNow;
                message.LastModifiedOn = DateTime.UtcNow;
                
                _dbContext.ClientSupportMessages.Add(message);
                
                // Update the ticket's last modified date
                var ticket = await _dbContext.ClientSupportTickets.FindAsync(message.ClientSupportTicketId);
                if (ticket != null)
                {
                    ticket.LastModifiedOn = DateTime.UtcNow;
                    _dbContext.ClientSupportTickets.Update(ticket);
                }
                
                await _dbContext.SaveChangesAsync();
                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding support message to ticket {TicketId}", message.ClientSupportTicketId);
                throw;
            }
        }

        public async Task<IEnumerable<SavingsGoal>> GetSavingsGoalsAsync(Guid profileId)
        {
            try
            {
                return await _dbContext.SavingsGoals
                    .Where(g => g.ClientPortalProfileId == profileId.ToString())
                    .Include(g => g.Transactions)
                    .OrderByDescending(g => g.CreatedOn)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving savings goals for profile {ProfileId}", profileId);
                throw;
            }
        }

        public async Task<SavingsGoal> CreateSavingsGoalAsync(SavingsGoal goal)
        {
            try
            {
                goal.CreatedOn = DateTime.UtcNow;
                goal.LastModifiedOn = DateTime.UtcNow;
                goal.Status = "active";
                
                _dbContext.SavingsGoals.Add(goal);
                await _dbContext.SaveChangesAsync();
                return goal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating savings goal for profile {ProfileId}", goal.ClientPortalProfileId);
                throw;
            }
        }

        public async Task<SavingsGoal> UpdateSavingsGoalAsync(SavingsGoal goal)
        {
            try
            {
                goal.LastModifiedOn = DateTime.UtcNow;
                _dbContext.SavingsGoals.Update(goal);
                await _dbContext.SaveChangesAsync();
                return goal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating savings goal {GoalId}", goal.Id);
                throw;
            }
        }

        public async Task<IEnumerable<SavedPayee>> GetSavedPayeesAsync(Guid profileId)
        {
            try
            {
                return await _dbContext.SavedPayees
                    .Where(p => p.ClientPortalProfileId == profileId.ToString())
                    .OrderByDescending(p => p.IsFavorite)
                    .ThenByDescending(p => p.LastUsed)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving saved payees for profile {ProfileId}", profileId);
                throw;
            }
        }

        public async Task<SavedPayee> CreateSavedPayeeAsync(SavedPayee payee)
        {
            try
            {
                payee.CreatedOn = DateTime.UtcNow;
                payee.LastModifiedOn = DateTime.UtcNow;
                
                _dbContext.SavedPayees.Add(payee);
                await _dbContext.SaveChangesAsync();
                return payee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating saved payee for profile {ProfileId}", payee.ClientPortalProfileId);
                throw;
            }
        }

        public async Task<SavedPayee> UpdateSavedPayeeAsync(SavedPayee payee)
        {
            try
            {
                payee.LastModifiedOn = DateTime.UtcNow;
                _dbContext.SavedPayees.Update(payee);
                await _dbContext.SaveChangesAsync();
                return payee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating saved payee {PayeeId}", payee.Id);
                throw;
            }
        }

        public async Task<bool> DeleteSavedPayeeAsync(Guid payeeId)
        {
            try
            {
                var payee = await _dbContext.SavedPayees.FindAsync(payeeId);
                if (payee != null)
                {
                    _dbContext.SavedPayees.Remove(payee);
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting saved payee {PayeeId}", payeeId);
                throw;
            }
        }

        #region Helper Methods
        
        private string GenerateTicketNumber()
        {
            // Format: TKT-[Year][Month][Day]-[Random 4 digits]
            var datePrefix = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = new Random();
            var randomDigits = random.Next(1000, 9999).ToString();
            
            return $"TKT-{datePrefix}-{randomDigits}";
        }
        
        private (string DeviceType, string Browser, string OperatingSystem) ParseUserAgent(string userAgent)
        {
            // Simplified user agent parsing - in a real system, use a library like UAParser or similar
            string deviceType = "desktop";
            string browser = "unknown";
            string os = "unknown";
            
            userAgent = userAgent.ToLower();
            
            // Device type detection
            if (userAgent.Contains("mobile") || userAgent.Contains("android") || userAgent.Contains("iphone"))
            {
                deviceType = "mobile";
            }
            else if (userAgent.Contains("ipad") || userAgent.Contains("tablet"))
            {
                deviceType = "tablet";
            }
            
            // Browser detection
            if (userAgent.Contains("chrome"))
            {
                browser = "Chrome";
            }
            else if (userAgent.Contains("firefox"))
            {
                browser = "Firefox";
            }
            else if (userAgent.Contains("safari") && !userAgent.Contains("chrome"))
            {
                browser = "Safari";
            }
            else if (userAgent.Contains("edge"))
            {
                browser = "Edge";
            }
            else if (userAgent.Contains("msie") || userAgent.Contains("trident"))
            {
                browser = "Internet Explorer";
            }
            
            // OS detection
            if (userAgent.Contains("windows"))
            {
                os = "Windows";
            }
            else if (userAgent.Contains("macintosh") || userAgent.Contains("mac os"))
            {
                os = "macOS";
            }
            else if (userAgent.Contains("linux") && !userAgent.Contains("android"))
            {
                os = "Linux";
            }
            else if (userAgent.Contains("android"))
            {
                os = "Android";
            }
            else if (userAgent.Contains("iphone") || userAgent.Contains("ipad"))
            {
                os = "iOS";
            }
            
            return (deviceType, browser, os);
        }
        
        #endregion
    }
}

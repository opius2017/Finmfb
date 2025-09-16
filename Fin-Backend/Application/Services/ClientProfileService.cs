using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FinTech.Domain.Entities.ClientPortal;
using FinTech.Domain.Entities.Customers;
using FinTech.Domain.Entities.Identity;
using FinTech.Application.DTOs.ClientPortal;
using FinTech.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace FinTech.Application.Services
{
    public interface IClientProfileService
    {
        // Profile Management
        Task<ClientPortalProfile> GetClientProfileAsync(Guid customerId);
        Task<ClientPortalProfile> UpdateClientProfileAsync(UpdateProfileDto profileDto, Guid customerId);
        Task<bool> UpdateProfilePictureAsync(ProfilePictureDto pictureDto, Guid customerId);
        Task<bool> DeleteProfilePictureAsync(Guid customerId);
        
        // Preference Management
        Task<NotificationPreferences> GetNotificationPreferencesAsync(Guid customerId);
        Task<NotificationPreferences> UpdateNotificationPreferencesAsync(NotificationPreferencesDto preferencesDto, Guid customerId);
        Task<DashboardPreferences> GetDashboardPreferencesAsync(Guid customerId);
        Task<DashboardPreferences> UpdateDashboardPreferencesAsync(DashboardPreferencesDto preferencesDto, Guid customerId);
        
        // Security Settings
        Task<SecurityPreferences> GetSecurityPreferencesAsync(Guid customerId);
        Task<SecurityPreferences> UpdateSecurityPreferencesAsync(SecurityPreferencesDto preferencesDto, Guid customerId);
        Task<IEnumerable<SecurityActivity>> GetRecentSecurityActivitiesAsync(Guid customerId, int count = 10);
        
        // Document Management
        Task<IEnumerable<ClientDocument>> GetClientDocumentsAsync(Guid customerId);
        Task<ClientDocument> UploadDocumentAsync(DocumentUploadDto documentDto, Guid customerId);
        Task<bool> DeleteDocumentAsync(Guid documentId, Guid customerId);
        Task<ClientDocument> GetDocumentAsync(Guid documentId, Guid customerId);
        
        // Activity History
        Task<IEnumerable<ClientPortalActivity>> GetClientActivitiesAsync(ActivityHistoryRequestDto requestDto, Guid customerId);
    }

    public class ClientProfileService : IClientProfileService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ILogger<ClientProfileService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileStorageService _fileStorage;

        public ClientProfileService(
            IApplicationDbContext dbContext,
            ILogger<ClientProfileService> logger,
            UserManager<ApplicationUser> userManager,
            IFileStorageService fileStorage)
        {
            _dbContext = dbContext;
            _logger = logger;
            _userManager = userManager;
            _fileStorage = fileStorage;
        }

        // Profile Management
        public async Task<ClientPortalProfile> GetClientProfileAsync(Guid customerId)
        {
            try
            {
                var profile = await _dbContext.ClientPortalProfiles
                    .Include(p => p.Customer)
                    .FirstOrDefaultAsync(p => p.CustomerId == customerId);
                
                if (profile == null)
                {
                    // Create a default profile if one doesn't exist
                    var customer = await _dbContext.Customers
                        .FirstOrDefaultAsync(c => c.Id == customerId);
                    
                    if (customer == null)
                    {
                        throw new KeyNotFoundException($"Customer with ID {customerId} not found.");
                    }
                    
                    profile = new ClientPortalProfile
                    {
                        CustomerId = customerId,
                        Customer = customer,
                        Language = "English",
                        Theme = "Light",
                        DateFormat = "dd/MM/yyyy",
                        TimeFormat = "12-hour",
                        IsProfileComplete = false,
                        LastLoginDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    _dbContext.ClientPortalProfiles.Add(profile);
                    await _dbContext.SaveChangesAsync();
                }
                
                return profile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client profile for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<ClientPortalProfile> UpdateClientProfileAsync(UpdateProfileDto profileDto, Guid customerId)
        {
            try
            {
                var profile = await _dbContext.ClientPortalProfiles
                    .FirstOrDefaultAsync(p => p.CustomerId == customerId);
                
                if (profile == null)
                {
                    // Create a new profile if one doesn't exist
                    profile = await GetClientProfileAsync(customerId);
                }
                
                // Update profile properties
                profile.Language = profileDto.Language ?? profile.Language;
                profile.Theme = profileDto.Theme ?? profile.Theme;
                profile.DateFormat = profileDto.DateFormat ?? profile.DateFormat;
                profile.TimeFormat = profileDto.TimeFormat ?? profile.TimeFormat;
                profile.UpdatedAt = DateTime.UtcNow;
                
                // Update customer information if provided
                var customer = await _dbContext.Customers
                    .FirstOrDefaultAsync(c => c.Id == customerId);
                
                if (customer != null)
                {
                    if (!string.IsNullOrEmpty(profileDto.PhoneNumber))
                    {
                        customer.PhoneNumber = profileDto.PhoneNumber;
                    }
                    
                    if (!string.IsNullOrEmpty(profileDto.Email))
                    {
                        customer.Email = profileDto.Email;
                        
                        // Update ApplicationUser email as well
                        var user = await _userManager.FindByIdAsync(customer.UserId.ToString());
                        if (user != null)
                        {
                            user.Email = profileDto.Email;
                            user.NormalizedEmail = profileDto.Email.ToUpper();
                            await _userManager.UpdateAsync(user);
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(profileDto.Address))
                    {
                        customer.Address = profileDto.Address;
                    }
                    
                    if (!string.IsNullOrEmpty(profileDto.City))
                    {
                        customer.City = profileDto.City;
                    }
                    
                    if (!string.IsNullOrEmpty(profileDto.State))
                    {
                        customer.State = profileDto.State;
                    }
                    
                    if (!string.IsNullOrEmpty(profileDto.ZipCode))
                    {
                        customer.ZipCode = profileDto.ZipCode;
                    }
                    
                    if (!string.IsNullOrEmpty(profileDto.Country))
                    {
                        customer.Country = profileDto.Country;
                    }
                    
                    customer.UpdatedAt = DateTime.UtcNow;
                }
                
                // Check if profile is now complete
                profile.IsProfileComplete = IsProfileComplete(profile, customer);
                
                // Create activity log
                var activity = new ClientPortalActivity
                {
                    CustomerId = customerId,
                    ActivityType = "Profile Update",
                    Description = "Updated profile information",
                    ActivityDate = DateTime.UtcNow,
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.ClientPortalActivities.Add(activity);
                
                await _dbContext.SaveChangesAsync();
                
                return profile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client profile for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> UpdateProfilePictureAsync(ProfilePictureDto pictureDto, Guid customerId)
        {
            try
            {
                var profile = await _dbContext.ClientPortalProfiles
                    .FirstOrDefaultAsync(p => p.CustomerId == customerId);
                
                if (profile == null)
                {
                    // Create a new profile if one doesn't exist
                    profile = await GetClientProfileAsync(customerId);
                }
                
                // Check if profile picture already exists
                if (!string.IsNullOrEmpty(profile.ProfilePictureUrl))
                {
                    // Delete the old profile picture
                    await _fileStorage.DeleteFileAsync(profile.ProfilePictureUrl);
                }
                
                // Upload the new profile picture
                string fileName = $"profile-{customerId}-{DateTime.UtcNow.Ticks}{Path.GetExtension(pictureDto.FileName)}";
                string fileUrl = await _fileStorage.UploadFileAsync(pictureDto.FileContent, fileName, "profile-pictures");
                
                // Update profile
                profile.ProfilePictureUrl = fileUrl;
                profile.UpdatedAt = DateTime.UtcNow;
                
                // Create activity log
                var activity = new ClientPortalActivity
                {
                    CustomerId = customerId,
                    ActivityType = "Profile Picture Update",
                    Description = "Updated profile picture",
                    ActivityDate = DateTime.UtcNow,
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.ClientPortalActivities.Add(activity);
                
                await _dbContext.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile picture for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> DeleteProfilePictureAsync(Guid customerId)
        {
            try
            {
                var profile = await _dbContext.ClientPortalProfiles
                    .FirstOrDefaultAsync(p => p.CustomerId == customerId);
                
                if (profile == null || string.IsNullOrEmpty(profile.ProfilePictureUrl))
                {
                    return false;
                }
                
                // Delete the profile picture
                await _fileStorage.DeleteFileAsync(profile.ProfilePictureUrl);
                
                // Update profile
                profile.ProfilePictureUrl = null;
                profile.UpdatedAt = DateTime.UtcNow;
                
                // Create activity log
                var activity = new ClientPortalActivity
                {
                    CustomerId = customerId,
                    ActivityType = "Profile Picture Delete",
                    Description = "Removed profile picture",
                    ActivityDate = DateTime.UtcNow,
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.ClientPortalActivities.Add(activity);
                
                await _dbContext.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting profile picture for customer {CustomerId}", customerId);
                throw;
            }
        }

        // Preference Management
        public async Task<NotificationPreferences> GetNotificationPreferencesAsync(Guid customerId)
        {
            try
            {
                var preferences = await _dbContext.NotificationPreferences
                    .FirstOrDefaultAsync(p => p.CustomerId == customerId);
                
                if (preferences == null)
                {
                    // Create default notification preferences if none exist
                    preferences = new NotificationPreferences
                    {
                        CustomerId = customerId,
                        EmailNotificationsEnabled = true,
                        SmsNotificationsEnabled = true,
                        PushNotificationsEnabled = true,
                        AccountActivityAlerts = true,
                        TransactionAlerts = true,
                        LoanPaymentReminders = true,
                        SecurityAlerts = true,
                        MarketingCommunications = false,
                        LowBalanceAlerts = true,
                        LowBalanceThreshold = 5000, // Default threshold
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    _dbContext.NotificationPreferences.Add(preferences);
                    await _dbContext.SaveChangesAsync();
                }
                
                return preferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification preferences for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<NotificationPreferences> UpdateNotificationPreferencesAsync(NotificationPreferencesDto preferencesDto, Guid customerId)
        {
            try
            {
                var preferences = await _dbContext.NotificationPreferences
                    .FirstOrDefaultAsync(p => p.CustomerId == customerId);
                
                if (preferences == null)
                {
                    // Create default preferences if none exist
                    preferences = await GetNotificationPreferencesAsync(customerId);
                }
                
                // Update preferences
                preferences.EmailNotificationsEnabled = preferencesDto.EmailNotificationsEnabled;
                preferences.SmsNotificationsEnabled = preferencesDto.SmsNotificationsEnabled;
                preferences.PushNotificationsEnabled = preferencesDto.PushNotificationsEnabled;
                preferences.AccountActivityAlerts = preferencesDto.AccountActivityAlerts;
                preferences.TransactionAlerts = preferencesDto.TransactionAlerts;
                preferences.LoanPaymentReminders = preferencesDto.LoanPaymentReminders;
                preferences.SecurityAlerts = preferencesDto.SecurityAlerts;
                preferences.MarketingCommunications = preferencesDto.MarketingCommunications;
                preferences.LowBalanceAlerts = preferencesDto.LowBalanceAlerts;
                
                if (preferencesDto.LowBalanceThreshold.HasValue)
                {
                    preferences.LowBalanceThreshold = preferencesDto.LowBalanceThreshold.Value;
                }
                
                preferences.UpdatedAt = DateTime.UtcNow;
                
                // Create activity log
                var activity = new ClientPortalActivity
                {
                    CustomerId = customerId,
                    ActivityType = "Notification Preferences Update",
                    Description = "Updated notification preferences",
                    ActivityDate = DateTime.UtcNow,
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.ClientPortalActivities.Add(activity);
                
                await _dbContext.SaveChangesAsync();
                
                return preferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification preferences for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<DashboardPreferences> GetDashboardPreferencesAsync(Guid customerId)
        {
            try
            {
                var preferences = await _dbContext.DashboardPreferences
                    .FirstOrDefaultAsync(p => p.CustomerId == customerId);
                
                if (preferences == null)
                {
                    // Create default dashboard preferences if none exist
                    preferences = new DashboardPreferences
                    {
                        CustomerId = customerId,
                        DefaultDashboard = "Overview",
                        ShowAccountBalances = true,
                        ShowRecentTransactions = true,
                        ShowUpcomingPayments = true,
                        ShowLoanOverview = true,
                        ShowSpendingAnalysis = true,
                        ShowQuickTransfer = true,
                        ShowSavingsGoals = true,
                        CustomWidgets = "[]", // Empty JSON array
                        WidgetLayout = "{\"layout\":\"default\"}", // Default layout JSON
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    _dbContext.DashboardPreferences.Add(preferences);
                    await _dbContext.SaveChangesAsync();
                }
                
                return preferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard preferences for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<DashboardPreferences> UpdateDashboardPreferencesAsync(DashboardPreferencesDto preferencesDto, Guid customerId)
        {
            try
            {
                var preferences = await _dbContext.DashboardPreferences
                    .FirstOrDefaultAsync(p => p.CustomerId == customerId);
                
                if (preferences == null)
                {
                    // Create default preferences if none exist
                    preferences = await GetDashboardPreferencesAsync(customerId);
                }
                
                // Update preferences
                if (!string.IsNullOrEmpty(preferencesDto.DefaultDashboard))
                {
                    preferences.DefaultDashboard = preferencesDto.DefaultDashboard;
                }
                
                preferences.ShowAccountBalances = preferencesDto.ShowAccountBalances;
                preferences.ShowRecentTransactions = preferencesDto.ShowRecentTransactions;
                preferences.ShowUpcomingPayments = preferencesDto.ShowUpcomingPayments;
                preferences.ShowLoanOverview = preferencesDto.ShowLoanOverview;
                preferences.ShowSpendingAnalysis = preferencesDto.ShowSpendingAnalysis;
                preferences.ShowQuickTransfer = preferencesDto.ShowQuickTransfer;
                preferences.ShowSavingsGoals = preferencesDto.ShowSavingsGoals;
                
                if (!string.IsNullOrEmpty(preferencesDto.CustomWidgets))
                {
                    preferences.CustomWidgets = preferencesDto.CustomWidgets;
                }
                
                if (!string.IsNullOrEmpty(preferencesDto.WidgetLayout))
                {
                    preferences.WidgetLayout = preferencesDto.WidgetLayout;
                }
                
                preferences.UpdatedAt = DateTime.UtcNow;
                
                // Create activity log
                var activity = new ClientPortalActivity
                {
                    CustomerId = customerId,
                    ActivityType = "Dashboard Preferences Update",
                    Description = "Updated dashboard preferences",
                    ActivityDate = DateTime.UtcNow,
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.ClientPortalActivities.Add(activity);
                
                await _dbContext.SaveChangesAsync();
                
                return preferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dashboard preferences for customer {CustomerId}", customerId);
                throw;
            }
        }

        // Security Settings
        public async Task<SecurityPreferences> GetSecurityPreferencesAsync(Guid customerId)
        {
            try
            {
                var preferences = await _dbContext.SecurityPreferences
                    .FirstOrDefaultAsync(p => p.CustomerId == customerId);
                
                if (preferences == null)
                {
                    // Get the user associated with the customer
                    var customer = await _dbContext.Customers
                        .FirstOrDefaultAsync(c => c.Id == customerId);
                    
                    if (customer == null)
                    {
                        throw new KeyNotFoundException($"Customer with ID {customerId} not found.");
                    }
                    
                    // Create default security preferences if none exist
                    preferences = new SecurityPreferences
                    {
                        CustomerId = customerId,
                        UserId = customer.UserId,
                        TwoFactorEnabled = false,
                        RememberDevice = true,
                        LoginNotifications = true,
                        TransactionSigningRequired = false,
                        AutoLockTimeout = 15, // 15 minutes
                        AllowMultipleDevices = true,
                        LastPasswordChangeDate = DateTime.UtcNow,
                        SecurityQuestionsConfigured = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    _dbContext.SecurityPreferences.Add(preferences);
                    await _dbContext.SaveChangesAsync();
                }
                
                return preferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security preferences for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<SecurityPreferences> UpdateSecurityPreferencesAsync(SecurityPreferencesDto preferencesDto, Guid customerId)
        {
            try
            {
                var preferences = await _dbContext.SecurityPreferences
                    .FirstOrDefaultAsync(p => p.CustomerId == customerId);
                
                if (preferences == null)
                {
                    // Create default preferences if none exist
                    preferences = await GetSecurityPreferencesAsync(customerId);
                }
                
                // Update the user's two-factor settings if needed
                if (preferences.TwoFactorEnabled != preferencesDto.TwoFactorEnabled)
                {
                    var customer = await _dbContext.Customers
                        .FirstOrDefaultAsync(c => c.Id == customerId);
                    
                    if (customer != null)
                    {
                        var user = await _userManager.FindByIdAsync(customer.UserId.ToString());
                        if (user != null)
                        {
                            user.TwoFactorEnabled = preferencesDto.TwoFactorEnabled;
                            await _userManager.UpdateAsync(user);
                        }
                    }
                }
                
                // Update preferences
                preferences.TwoFactorEnabled = preferencesDto.TwoFactorEnabled;
                preferences.RememberDevice = preferencesDto.RememberDevice;
                preferences.LoginNotifications = preferencesDto.LoginNotifications;
                preferences.TransactionSigningRequired = preferencesDto.TransactionSigningRequired;
                preferences.AllowMultipleDevices = preferencesDto.AllowMultipleDevices;
                
                if (preferencesDto.AutoLockTimeout.HasValue)
                {
                    preferences.AutoLockTimeout = preferencesDto.AutoLockTimeout.Value;
                }
                
                preferences.UpdatedAt = DateTime.UtcNow;
                
                // Create activity log
                var activity = new ClientPortalActivity
                {
                    CustomerId = customerId,
                    ActivityType = "Security Preferences Update",
                    Description = "Updated security preferences",
                    ActivityDate = DateTime.UtcNow,
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.ClientPortalActivities.Add(activity);
                
                // Create security activity log
                var securityActivity = new SecurityActivity
                {
                    UserId = preferences.UserId,
                    CustomerId = customerId,
                    ActivityType = "Security Settings Update",
                    Description = "Updated security settings",
                    IpAddress = preferencesDto.IpAddress,
                    UserAgent = preferencesDto.UserAgent,
                    ActivityDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.SecurityActivities.Add(securityActivity);
                
                await _dbContext.SaveChangesAsync();
                
                return preferences;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating security preferences for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<IEnumerable<SecurityActivity>> GetRecentSecurityActivitiesAsync(Guid customerId, int count = 10)
        {
            try
            {
                var customer = await _dbContext.Customers
                    .FirstOrDefaultAsync(c => c.Id == customerId);
                
                if (customer == null)
                {
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found.");
                }
                
                var activities = await _dbContext.SecurityActivities
                    .Where(a => a.CustomerId == customerId)
                    .OrderByDescending(a => a.ActivityDate)
                    .Take(count)
                    .ToListAsync();
                
                return activities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving security activities for customer {CustomerId}", customerId);
                throw;
            }
        }

        // Document Management
        public async Task<IEnumerable<ClientDocument>> GetClientDocumentsAsync(Guid customerId)
        {
            try
            {
                return await _dbContext.ClientDocuments
                    .Where(d => d.CustomerId == customerId)
                    .OrderByDescending(d => d.UploadDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving documents for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<ClientDocument> UploadDocumentAsync(DocumentUploadDto documentDto, Guid customerId)
        {
            try
            {
                // Upload document to storage
                string fileName = $"{documentDto.DocumentType}-{customerId}-{DateTime.UtcNow.Ticks}{Path.GetExtension(documentDto.FileName)}";
                string fileUrl = await _fileStorage.UploadFileAsync(documentDto.FileContent, fileName, "client-documents");
                
                // Create document record
                var document = new ClientDocument
                {
                    CustomerId = customerId,
                    DocumentType = documentDto.DocumentType,
                    DocumentName = documentDto.DocumentName,
                    FileName = documentDto.FileName,
                    FileSize = documentDto.FileSize,
                    FileType = Path.GetExtension(documentDto.FileName).TrimStart('.'),
                    FileUrl = fileUrl,
                    Description = documentDto.Description,
                    UploadDate = DateTime.UtcNow,
                    Status = "Pending Review",
                    IsVerified = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.ClientDocuments.Add(document);
                
                // Create activity log
                var activity = new ClientPortalActivity
                {
                    CustomerId = customerId,
                    ActivityType = "Document Upload",
                    Description = $"Uploaded document: {documentDto.DocumentName}",
                    ActivityDate = DateTime.UtcNow,
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.ClientPortalActivities.Add(activity);
                
                await _dbContext.SaveChangesAsync();
                
                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> DeleteDocumentAsync(Guid documentId, Guid customerId)
        {
            try
            {
                var document = await _dbContext.ClientDocuments
                    .FirstOrDefaultAsync(d => d.Id == documentId);
                
                if (document == null)
                {
                    throw new KeyNotFoundException($"Document with ID {documentId} not found.");
                }
                
                if (document.CustomerId != customerId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to delete this document.");
                }
                
                // Cannot delete verified documents
                if (document.IsVerified)
                {
                    throw new InvalidOperationException("Cannot delete a verified document.");
                }
                
                // Delete the file from storage
                await _fileStorage.DeleteFileAsync(document.FileUrl);
                
                // Remove the document record
                _dbContext.ClientDocuments.Remove(document);
                
                // Create activity log
                var activity = new ClientPortalActivity
                {
                    CustomerId = customerId,
                    ActivityType = "Document Delete",
                    Description = $"Deleted document: {document.DocumentName}",
                    ActivityDate = DateTime.UtcNow,
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _dbContext.ClientPortalActivities.Add(activity);
                
                await _dbContext.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {DocumentId} for customer {CustomerId}", documentId, customerId);
                throw;
            }
        }

        public async Task<ClientDocument> GetDocumentAsync(Guid documentId, Guid customerId)
        {
            try
            {
                var document = await _dbContext.ClientDocuments
                    .FirstOrDefaultAsync(d => d.Id == documentId);
                
                if (document == null)
                {
                    throw new KeyNotFoundException($"Document with ID {documentId} not found.");
                }
                
                if (document.CustomerId != customerId)
                {
                    throw new UnauthorizedAccessException("You do not have permission to access this document.");
                }
                
                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document {DocumentId} for customer {CustomerId}", documentId, customerId);
                throw;
            }
        }

        // Activity History
        public async Task<IEnumerable<ClientPortalActivity>> GetClientActivitiesAsync(ActivityHistoryRequestDto requestDto, Guid customerId)
        {
            try
            {
                // Base query for client activities
                var query = _dbContext.ClientPortalActivities
                    .Where(a => a.CustomerId == customerId);
                
                // Apply filters if provided
                if (requestDto.FromDate.HasValue)
                {
                    query = query.Where(a => a.ActivityDate >= requestDto.FromDate.Value);
                }
                
                if (requestDto.ToDate.HasValue)
                {
                    query = query.Where(a => a.ActivityDate <= requestDto.ToDate.Value);
                }
                
                if (!string.IsNullOrEmpty(requestDto.ActivityType))
                {
                    query = query.Where(a => a.ActivityType == requestDto.ActivityType);
                }
                
                // Get paginated results
                var activities = await query
                    .OrderByDescending(a => a.ActivityDate)
                    .Skip((requestDto.Page - 1) * requestDto.PageSize)
                    .Take(requestDto.PageSize)
                    .ToListAsync();
                
                return activities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity history for customer {CustomerId}", customerId);
                throw;
            }
        }

        // Helper method to check if profile is complete
        private bool IsProfileComplete(ClientPortalProfile profile, Customer customer)
        {
            if (customer == null)
            {
                return false;
            }
            
            // Check if all required fields are filled
            return !string.IsNullOrEmpty(customer.PhoneNumber) &&
                   !string.IsNullOrEmpty(customer.Email) &&
                   !string.IsNullOrEmpty(customer.Address) &&
                   !string.IsNullOrEmpty(customer.City) &&
                   !string.IsNullOrEmpty(customer.State) &&
                   !string.IsNullOrEmpty(customer.ZipCode) &&
                   !string.IsNullOrEmpty(customer.Country);
        }
    }

    // Interface for file storage service
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(byte[] fileContent, string fileName, string containerName);
        Task<bool> DeleteFileAsync(string fileUrl);
        Task<byte[]> DownloadFileAsync(string fileUrl);
    }
}
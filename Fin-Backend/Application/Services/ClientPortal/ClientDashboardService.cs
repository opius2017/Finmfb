using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Domain.Entities.ClientPortal;
using FinTech.Core.Application.Common.Interfaces;

namespace FinTech.Core.Application.Services
{
    public interface IClientDashboardService
    {
        Task<ClientDashboardDto> GetDashboardDataAsync(Guid customerId);
        Task<DashboardPreferencesDto> GetDashboardPreferencesAsync(Guid customerId);
        Task<DashboardPreferencesDto> UpdateDashboardPreferencesAsync(Guid customerId, DashboardPreferencesUpdateDto preferencesDto);
    }

    public class ClientDashboardService : IClientDashboardService
    {
        private readonly IApplicationDbContext _context;
        private readonly IClientLoanService _loanService;
        private readonly IClientPaymentService _paymentService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ClientDashboardService> _logger;

        public ClientDashboardService(
            IApplicationDbContext context,
            IClientLoanService loanService,
            IClientPaymentService paymentService,
            INotificationService notificationService,
            ILogger<ClientDashboardService> logger)
        {
            _context = context;
            _loanService = loanService;
            _paymentService = paymentService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<ClientDashboardDto> GetDashboardDataAsync(Guid customerId)
        {
            try
            {
                // Get customer and preferences
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == customerId);

                if (customer == null)
                {
                    throw new KeyNotFoundException($"Customer with ID {customerId} not found.");
                }

                // Get client portal profile
                var profile = await _context.ClientPortalProfiles
                    .Include(p => p.DashboardPreferences)
                    .FirstOrDefaultAsync(p => p.CustomerId == customerId);

                if (profile == null)
                {
                    throw new KeyNotFoundException($"Client portal profile for customer {customerId} not found.");
                }

                // Create dashboard DTO
                var dashboardDto = new ClientDashboardDto
                {
                    CustomerName = $"{customer.FirstName} {customer.LastName}",
                    LastLoginDate = profile.LastLoginDate,
                    Preferences = new DashboardPreferencesDto
                    {
                        Id = profile.DashboardPreferences?.Id ?? Guid.Empty,
                        ShowAccountBalances = profile.DashboardPreferences?.ShowAccountBalances ?? true,
                        ShowRecentTransactions = profile.DashboardPreferences?.ShowRecentTransactions ?? true,
                        ShowUpcomingPayments = profile.DashboardPreferences?.ShowUpcomingPayments ?? true,
                        ShowLoanStatus = profile.DashboardPreferences?.ShowLoanStatus ?? true,
                        ShowSavingsGoals = profile.DashboardPreferences?.ShowSavingsGoals ?? true,
                        ShowQuickActions = profile.DashboardPreferences?.ShowQuickActions ?? true,
                        ShowFinancialInsights = profile.DashboardPreferences?.ShowFinancialInsights ?? true,
                        Layout = profile.DashboardPreferences?.Layout ?? "default",
                        VisibleWidgets = profile.DashboardPreferences?.VisibleWidgets?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? new string[0],
                        WidgetOrder = profile.DashboardPreferences?.WidgetOrder?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? new string[0]
                    }
                };

                // Get deposit accounts
                var depositAccounts = await _context.DepositAccounts
                    .Where(a => a.CustomerId == customerId && a.Status == "Active")
                    .ToListAsync();

                dashboardDto.AccountBalances = depositAccounts.Select(a => new AccountBalanceDto
                {
                    AccountId = a.Id,
                    AccountNumber = a.AccountNumber,
                    AccountType = a.AccountType,
                    Currency = a.Currency,
                    CurrentBalance = a.CurrentBalance,
                    AvailableBalance = a.AvailableBalance,
                    Status = a.Status
                }).ToList();

                // Get recent transactions
                if (profile.DashboardPreferences?.ShowRecentTransactions ?? true)
                {
                    var recentTransactions = await _context.DepositTransactions
                        .Where(t => depositAccounts.Select(a => a.Id).Contains(t.AccountId))
                        .OrderByDescending(t => t.TransactionDate)
                        .Take(5)
                        .ToListAsync();

                    dashboardDto.RecentTransactions = recentTransactions.Select(t => new TransactionSummaryDto
                    {
                        Id = t.Id,
                        AccountId = t.AccountId,
                        AccountNumber = depositAccounts.FirstOrDefault(a => a.Id == t.AccountId)?.AccountNumber,
                        TransactionDate = t.TransactionDate,
                        Description = t.Description,
                        Amount = t.Amount,
                        Currency = t.Currency,
                        TransactionType = t.TransactionType,
                        Category = t.Category,
                        Status = t.Status
                    }).ToList();
                }

                // Get loan accounts
                if (profile.DashboardPreferences?.ShowLoanStatus ?? true)
                {
                    var loanAccounts = await _context.LoanAccounts
                        .Where(l => l.CustomerId == customerId && l.Status != "Closed")
                        .ToListAsync();

                    dashboardDto.LoanAccounts = loanAccounts.Select(l => new LoanAccountSummaryDto
                    {
                        Id = l.Id,
                        LoanNumber = l.LoanNumber,
                        LoanType = l.LoanType,
                        PrincipalAmount = l.PrincipalAmount,
                        OutstandingBalance = l.OutstandingBalance,
                        NextPaymentAmount = l.NextPaymentAmount,
                        NextPaymentDate = l.NextPaymentDate,
                        Status = l.Status
                    }).ToList();
                }

                // Get upcoming payments
                if (profile.DashboardPreferences?.ShowUpcomingPayments ?? true)
                {
                    var upcomingPayments = await _context.RecurringPayments
                        .Where(p => p.CustomerId == customerId && p.Status == "Active")
                        .OrderBy(p => p.NextPaymentDate)
                        .Take(5)
                        .ToListAsync();

                    dashboardDto.UpcomingPayments = upcomingPayments.Select(p => new UpcomingPaymentDto
                    {
                        Id = p.Id,
                        PaymentName = p.PaymentName,
                        Amount = p.Amount,
                        Currency = p.Currency,
                        NextPaymentDate = p.NextPaymentDate,
                        PaymentType = p.PaymentType,
                        Status = p.Status
                    }).ToList();
                }

                // Get savings goals
                if (profile.DashboardPreferences?.ShowSavingsGoals ?? true)
                {
                    var savingsGoals = await _context.SavingsGoals
                        .Where(g => g.CustomerId == customerId && g.Status == "Active")
                        .OrderBy(g => g.TargetDate)
                        .Take(5)
                        .ToListAsync();

                    dashboardDto.SavingsGoals = savingsGoals.Select(g => new SavingsGoalSummaryDto
                    {
                        Id = g.Id,
                        GoalName = g.GoalName,
                        TargetAmount = g.TargetAmount,
                        CurrentAmount = g.CurrentAmount,
                        Currency = g.Currency,
                        TargetDate = g.TargetDate,
                        Progress = g.TargetAmount > 0 ? (int)((g.CurrentAmount / g.TargetAmount) * 100) : 0
                    }).ToList();
                }

                // Get notifications
                var notificationCounts = await _notificationService.GetNotificationCountsAsync(customerId);
                dashboardDto.NotificationCounts = notificationCounts;

                // Get recent support tickets
                var recentTickets = await _context.ClientSupportTickets
                    .Where(t => t.CustomerId == customerId)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(3)
                    .ToListAsync();

                dashboardDto.RecentSupportTickets = recentTickets.Select(t => new SupportTicketSummaryDto
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber,
                    Subject = t.Subject,
                    Status = t.Status,
                    CreatedDate = t.CreatedAt
                }).ToList();

                return dashboardDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard data for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<DashboardPreferencesDto> GetDashboardPreferencesAsync(Guid customerId)
        {
            try
            {
                var profile = await _context.ClientPortalProfiles
                    .Include(p => p.DashboardPreferences)
                    .FirstOrDefaultAsync(p => p.CustomerId == customerId);

                if (profile == null)
                {
                    throw new KeyNotFoundException($"Client portal profile for customer {customerId} not found.");
                }

                if (profile.DashboardPreferences == null)
                {
                    // Create default preferences if none exist
                    var defaultPreferences = new DashboardPreferences
                    {
                        ClientPortalProfileId = profile.Id,
                        ShowAccountBalances = true,
                        ShowRecentTransactions = true,
                        ShowUpcomingPayments = true,
                        ShowLoanStatus = true,
                        ShowSavingsGoals = true,
                        ShowQuickActions = true,
                        ShowFinancialInsights = true,
                        Layout = "default",
                        VisibleWidgets = "accounts,transactions,loans,payments,goals",
                        WidgetOrder = "accounts,transactions,loans,payments,goals",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.DashboardPreferences.Add(defaultPreferences);
                    await _context.SaveChangesAsync();

                    profile.DashboardPreferences = defaultPreferences;
                }

                return new DashboardPreferencesDto
                {
                    Id = profile.DashboardPreferences.Id,
                    ShowAccountBalances = profile.DashboardPreferences.ShowAccountBalances,
                    ShowRecentTransactions = profile.DashboardPreferences.ShowRecentTransactions,
                    ShowUpcomingPayments = profile.DashboardPreferences.ShowUpcomingPayments,
                    ShowLoanStatus = profile.DashboardPreferences.ShowLoanStatus,
                    ShowSavingsGoals = profile.DashboardPreferences.ShowSavingsGoals,
                    ShowQuickActions = profile.DashboardPreferences.ShowQuickActions,
                    ShowFinancialInsights = profile.DashboardPreferences.ShowFinancialInsights,
                    Layout = profile.DashboardPreferences.Layout,
                    VisibleWidgets = profile.DashboardPreferences.VisibleWidgets?.Split(',', StringSplitOptions.RemoveEmptyEntries),
                    WidgetOrder = profile.DashboardPreferences.WidgetOrder?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard preferences for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<DashboardPreferencesDto> UpdateDashboardPreferencesAsync(Guid customerId, DashboardPreferencesUpdateDto preferencesDto)
        {
            try
            {
                var profile = await _context.ClientPortalProfiles
                    .Include(p => p.DashboardPreferences)
                    .FirstOrDefaultAsync(p => p.CustomerId == customerId);

                if (profile == null)
                {
                    throw new KeyNotFoundException($"Client portal profile for customer {customerId} not found.");
                }

                if (profile.DashboardPreferences == null)
                {
                    // Create preferences if none exist
                    profile.DashboardPreferences = new DashboardPreferences
                    {
                        ClientPortalProfileId = profile.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.DashboardPreferences.Add(profile.DashboardPreferences);
                }

                // Update preferences
                if (preferencesDto.ShowAccountBalances.HasValue)
                    profile.DashboardPreferences.ShowAccountBalances = preferencesDto.ShowAccountBalances.Value;
                
                if (preferencesDto.ShowRecentTransactions.HasValue)
                    profile.DashboardPreferences.ShowRecentTransactions = preferencesDto.ShowRecentTransactions.Value;
                
                if (preferencesDto.ShowUpcomingPayments.HasValue)
                    profile.DashboardPreferences.ShowUpcomingPayments = preferencesDto.ShowUpcomingPayments.Value;
                
                if (preferencesDto.ShowLoanStatus.HasValue)
                    profile.DashboardPreferences.ShowLoanStatus = preferencesDto.ShowLoanStatus.Value;
                
                if (preferencesDto.ShowSavingsGoals.HasValue)
                    profile.DashboardPreferences.ShowSavingsGoals = preferencesDto.ShowSavingsGoals.Value;
                
                if (preferencesDto.ShowQuickActions.HasValue)
                    profile.DashboardPreferences.ShowQuickActions = preferencesDto.ShowQuickActions.Value;
                
                if (preferencesDto.ShowFinancialInsights.HasValue)
                    profile.DashboardPreferences.ShowFinancialInsights = preferencesDto.ShowFinancialInsights.Value;
                
                if (preferencesDto.Layout != null)
                    profile.DashboardPreferences.Layout = preferencesDto.Layout;
                
                if (preferencesDto.VisibleWidgets != null)
                    profile.DashboardPreferences.VisibleWidgets = string.Join(",", preferencesDto.VisibleWidgets);
                
                if (preferencesDto.WidgetOrder != null)
                    profile.DashboardPreferences.WidgetOrder = string.Join(",", preferencesDto.WidgetOrder);
                
                profile.DashboardPreferences.UpdatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                return await GetDashboardPreferencesAsync(customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating dashboard preferences for customer {CustomerId}", customerId);
                throw;
            }
        }
    }
}

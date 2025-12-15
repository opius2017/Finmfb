using System;
using System.Collections.Generic;
using AutoMapper;
using FinTech.Core.Application.DTOs.ClientPortal;
using FinTech.Core.Domain.Entities.ClientPortal;

namespace FinTech.Core.Application.Mappings
{
    public class ClientPortalMappingProfile : Profile
    {
        public ClientPortalMappingProfile()
        {
            // Client Portal Profile Mappings
            CreateMap<ClientPortalProfile, ClientPortalProfileDto>();
            
            // Notification Preferences Mappings
            CreateMap<NotificationPreferences, NotificationPreferencesDto>();
            CreateMap<NotificationPreferencesUpdateDto, NotificationPreferences>()
                .ForAllMembers(opts => opts
                    .Condition((src, dest, srcMember) => srcMember != null));
            
            // Dashboard Preferences Mappings
            CreateMap<DashboardPreferences, DashboardPreferencesDto>();
            CreateMap<DashboardPreferencesUpdateDto, DashboardPreferences>()
                .ForAllMembers(opts => opts
                    .Condition((src, dest, srcMember) => srcMember != null));
            
            // Session Mappings
            CreateMap<ClientSession, ClientPortalSessionDto>();
            CreateMap<ClientPortalActivity, ClientPortalActivityDto>();
            
            // Document Mappings
            CreateMap<ClientDocument, ClientDocumentDto>();
            
            // Support Ticket Mappings
            CreateMap<ClientSupportTicket, ClientSupportTicketDto>();
            CreateMap<ClientSupportMessage, ClientSupportMessageDto>();
            
            // Savings Goal Mappings
            CreateMap<SavingsGoal, SavingsGoalDto>();
            CreateMap<CreateSavingsGoalDto, SavingsGoal>();
            CreateMap<SavingsGoalTransaction, SavingsGoalTransactionDto>();
            
            // Payee Mappings
            CreateMap<SavedPayee, SavedPayeeDto>();
            CreateMap<CreateSavedPayeeDto, SavedPayee>();
            
            // Transfer Template Mappings
            CreateMap<SavedTransferTemplate, SavedTransferTemplateDto>();
            CreateMap<CreateSavedTransferTemplateDto, SavedTransferTemplate>();
            
            // Notification Mappings
            CreateMap<ClientNotification, ClientNotificationDto>();
        }
    }
}

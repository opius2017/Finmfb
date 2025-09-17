using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using FinTech.Application.DTOs.Common;
using FinTech.Application.DTOs.ClientPortal;
using FinTech.Core.Application.DTOs.Common;
using FinTech.Application.Services;
using FinTech.Domain.Entities.ClientPortal;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace FinTech.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientPortalController : ControllerBase
    {
        private readonly IClientPortalService _clientPortalService;
        private readonly ILogger<ClientPortalController> _logger;
        private readonly IMapper _mapper;

        public ClientPortalController(
            IClientPortalService clientPortalService,
            ILogger<ClientPortalController> logger,
            IMapper mapper)
        // ...existing code...
        {
            _clientPortalService = clientPortalService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("profile")]
    public async Task<ActionResult<BaseResponse<ClientPortalProfileDto>>> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                    var profile = await _clientPortalService.GetProfileAsync();

                if (profile == null)
                    return NotFound(BaseResponse<ClientPortalProfileDto>.ErrorResponse("Profile not found"));
                var profileDto = _mapper.Map<ClientPortalProfileDto>(profile);
                return Ok(BaseResponse<ClientPortalProfileDto>.SuccessResponse(profileDto, "Profile retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client profile");
                return StatusCode(500, BaseResponse<ClientPortalProfileDto>.ErrorResponse("Internal server error"));
                {
                    Success = false,
                    Message = "An error occurred while retrieving the client profile"
                });
            }
        }

        [HttpPut("profile")]
        public async Task<ActionResult<BaseResponse<ClientPortalProfileDto>>> UpdateProfile(UpdateClientPortalProfileDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var existingProfile = await _clientPortalService.GetClientProfileAsync(userId);

                if (existingProfile == null)
                {
                    return NotFound(new BaseResponse<ClientPortalProfileDto>
                    {
                        Success = false,
                        Message = "Client profile not found"
                    });
                }

                // Update profile properties
                existingProfile.PreferredLanguage = updateDto.PreferredLanguage ?? existingProfile.PreferredLanguage;
                existingProfile.TimeZone = updateDto.TimeZone ?? existingProfile.TimeZone;
                existingProfile.DarkModeEnabled = updateDto.DarkModeEnabled ?? existingProfile.DarkModeEnabled;
                existingProfile.NewsletterSubscribed = updateDto.NewsletterSubscribed ?? existingProfile.NewsletterSubscribed;
                existingProfile.MarketingNotificationsEnabled = updateDto.MarketingNotificationsEnabled ?? existingProfile.MarketingNotificationsEnabled;
                existingProfile.PushNotificationToken = updateDto.PushNotificationToken ?? existingProfile.PushNotificationToken;

                // Update notification preferences if provided
                if (updateDto.NotificationPreferences != null)
                {
                    _mapper.Map(updateDto.NotificationPreferences, existingProfile.NotificationPreferences);
                }

                // Update dashboard preferences if provided
                if (updateDto.DashboardPreferences != null)
                {
                    _mapper.Map(updateDto.DashboardPreferences, existingProfile.DashboardPreferences);
                }

                var updatedProfile = await _clientPortalService.UpdateClientProfileAsync(existingProfile);
                var profileDto = _mapper.Map<ClientPortalProfileDto>(updatedProfile);

                return Ok(new BaseResponse<ClientPortalProfileDto>
                {
                    Success = true,
                    Data = profileDto,
                    Message = "Profile updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client profile");
                return StatusCode(500, new BaseResponse<ClientPortalProfileDto>
                {
                    Success = false,
                    Message = "An error occurred while updating the profile"
                });
            }
        }

        [HttpGet("sessions")]
        public async Task<ActionResult<BaseResponse<List<ClientPortalSessionDto>>>> GetSessions([FromQuery] int limit = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                var profile = await _clientPortalService.GetClientProfileAsync(userId);

                if (profile == null)
                {
                    return NotFound(new BaseResponse<List<ClientPortalSessionDto>>
                    {
                        Success = false,
                        Message = "Client profile not found"
                    });
                }

                var sessions = await _clientPortalService.GetClientSessionsAsync(profile.Id, limit);
                var sessionDtos = _mapper.Map<List<ClientPortalSessionDto>>(sessions);

                return Ok(new BaseResponse<List<ClientPortalSessionDto>>
                {
                    Success = true,
                    Data = sessionDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client sessions");
                return StatusCode(500, new BaseResponse<List<ClientPortalSessionDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the sessions"
                });
            }
        }

        [HttpPost("sessions/end/{sessionId}")]
        public async Task<ActionResult<BaseResponse<bool>>> EndSession(Guid sessionId)
        {
            try
            {
                await _clientPortalService.EndSessionAsync(sessionId);

                return Ok(new BaseResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Session ended successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending client session");
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while ending the session"
                });
            }
        }

        [HttpGet("documents")]
        public async Task<ActionResult<BaseResponse<List<ClientDocumentDto>>>> GetDocuments()
        {
            try
            {
                var userId = GetCurrentUserId();
                var profile = await _clientPortalService.GetClientProfileAsync(userId);

                if (profile == null)
                {
                    return NotFound(new BaseResponse<List<ClientDocumentDto>>
                    {
                        Success = false,
                        Message = "Client profile not found"
                    });
                }

                var documents = await _clientPortalService.GetClientDocumentsAsync(profile.Id);
                var documentDtos = _mapper.Map<List<ClientDocumentDto>>(documents);

                return Ok(new BaseResponse<List<ClientDocumentDto>>
                {
                    Success = true,
                    Data = documentDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client documents");
                return StatusCode(500, new BaseResponse<List<ClientDocumentDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the documents"
                });
            }
        }

        [HttpPost("documents")]
        public async Task<ActionResult<BaseResponse<ClientDocumentDto>>> UploadDocument([FromForm] UploadDocumentDto uploadDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var profile = await _clientPortalService.GetClientProfileAsync(userId);

                if (profile == null)
                {
                    return NotFound(new BaseResponse<ClientDocumentDto>
                    {
                        Success = false,
                        Message = "Client profile not found"
                    });
                }

                if (uploadDto.File == null || uploadDto.File.Length == 0)
                {
                    return BadRequest(new BaseResponse<ClientDocumentDto>
                    {
                        Success = false,
                        Message = "No file was uploaded"
                    });
                }

                // TODO: Implement file storage service to handle the actual file upload
                // For now, we'll simulate the file storage and just create the document record

                var document = new ClientDocument
                {
                    ClientPortalProfileId = profile.Id,
                    DocumentName = uploadDto.DocumentName,
                    DocumentType = uploadDto.DocumentType,
                    FileName = uploadDto.File.FileName,
                    MimeType = uploadDto.File.ContentType,
                    FileSize = uploadDto.File.Length,
                    StorageProvider = "local", // This would be replaced with the actual storage provider
                    FilePath = $"/documents/{Guid.NewGuid()}_{uploadDto.File.FileName}", // This would be replaced with the actual file path
                    IsSharedByBank = false
                };

                var savedDocument = await _clientPortalService.UploadDocumentAsync(document);
                var documentDto = _mapper.Map<ClientDocumentDto>(savedDocument);

                return Ok(new BaseResponse<ClientDocumentDto>
                {
                    Success = true,
                    Data = documentDto,
                    Message = "Document uploaded successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                return StatusCode(500, new BaseResponse<ClientDocumentDto>
                {
                    Success = false,
                    Message = "An error occurred while uploading the document"
                });
            }
        }

        [HttpGet("support-tickets")]
        public async Task<ActionResult<BaseResponse<List<ClientSupportTicketDto>>>> GetSupportTickets()
        {
            try
            {
                var userId = GetCurrentUserId();
                var profile = await _clientPortalService.GetClientProfileAsync(userId);

                if (profile == null)
                {
                    return NotFound(new BaseResponse<List<ClientSupportTicketDto>>
                    {
                        Success = false,
                        Message = "Client profile not found"
                    });
                }

                var tickets = await _clientPortalService.GetSupportTicketsAsync(profile.Id);
                var ticketDtos = _mapper.Map<List<ClientSupportTicketDto>>(tickets);

                return Ok(new BaseResponse<List<ClientSupportTicketDto>>
                {
                    Success = true,
                    Data = ticketDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving support tickets");
                return StatusCode(500, new BaseResponse<List<ClientSupportTicketDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving support tickets"
                });
            }
        }

        [HttpPost("support-tickets")]
        public async Task<ActionResult<BaseResponse<ClientSupportTicketDto>>> CreateSupportTicket(CreateSupportTicketDto ticketDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var profile = await _clientPortalService.GetClientProfileAsync(userId);

                if (profile == null)
                {
                    return NotFound(new BaseResponse<ClientSupportTicketDto>
                    {
                        Success = false,
                        Message = "Client profile not found"
                    });
                }

                var ticket = new ClientSupportTicket
                {
                    ClientPortalProfileId = profile.Id,
                    Subject = ticketDto.Subject,
                    Description = ticketDto.Description,
                    Category = ticketDto.Category,
                    Priority = ticketDto.Priority
                };

                var createdTicket = await _clientPortalService.CreateSupportTicketAsync(ticket);

                // Add the initial message from the client
                if (!string.IsNullOrEmpty(ticketDto.InitialMessage))
                {
                    var message = new ClientSupportMessage
                    {
                        ClientSupportTicketId = createdTicket.Id,
                        Message = ticketDto.InitialMessage,
                        IsFromClient = true,
                        SentById = userId,
                        SenderName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Client"
                    };

                    await _clientPortalService.AddSupportMessageAsync(message);
                }

                var ticketResponse = _mapper.Map<ClientSupportTicketDto>(createdTicket);

                return Ok(new BaseResponse<ClientSupportTicketDto>
                {
                    Success = true,
                    Data = ticketResponse,
                    Message = "Support ticket created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating support ticket");
                return StatusCode(500, new BaseResponse<ClientSupportTicketDto>
                {
                    Success = false,
                    Message = "An error occurred while creating the support ticket"
                });
            }
        }

        [HttpPost("support-tickets/{ticketId}/messages")]
        public async Task<ActionResult<BaseResponse<ClientSupportMessageDto>>> AddSupportMessage(Guid ticketId, AddSupportMessageDto messageDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var message = new ClientSupportMessage
                {
                    ClientSupportTicketId = ticketId,
                    Message = messageDto.Message,
                    IsFromClient = true,
                    SentById = userId,
                    SenderName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Client"
                };

                var createdMessage = await _clientPortalService.AddSupportMessageAsync(message);
                var messageResponse = _mapper.Map<ClientSupportMessageDto>(createdMessage);

                return Ok(new BaseResponse<ClientSupportMessageDto>
                {
                    Success = true,
                    Data = messageResponse,
                    Message = "Message added successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding support message");
                return StatusCode(500, new BaseResponse<ClientSupportMessageDto>
                {
                    Success = false,
                    Message = "An error occurred while adding the message"
                });
            }
        }

        [HttpGet("savings-goals")]
        public async Task<ActionResult<BaseResponse<List<SavingsGoalDto>>>> GetSavingsGoals()
        {
            try
            {
                var userId = GetCurrentUserId();
                var profile = await _clientPortalService.GetClientProfileAsync(userId);

                if (profile == null)
                {
                    return NotFound(new BaseResponse<List<SavingsGoalDto>>
                    {
                        Success = false,
                        Message = "Client profile not found"
                    });
                }

                var goals = await _clientPortalService.GetSavingsGoalsAsync(profile.Id);
                var goalDtos = _mapper.Map<List<SavingsGoalDto>>(goals);

                return Ok(new BaseResponse<List<SavingsGoalDto>>
                {
                    Success = true,
                    Data = goalDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving savings goals");
                return StatusCode(500, new BaseResponse<List<SavingsGoalDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving savings goals"
                });
            }
        }

        [HttpPost("savings-goals")]
        public async Task<ActionResult<BaseResponse<SavingsGoalDto>>> CreateSavingsGoal(CreateSavingsGoalDto goalDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var profile = await _clientPortalService.GetClientProfileAsync(userId);

                if (profile == null)
                {
                    return NotFound(new BaseResponse<SavingsGoalDto>
                    {
                        Success = false,
                        Message = "Client profile not found"
                    });
                }

                var goal = _mapper.Map<SavingsGoal>(goalDto);
                goal.ClientPortalProfileId = profile.Id;

                var createdGoal = await _clientPortalService.CreateSavingsGoalAsync(goal);
                var goalResponse = _mapper.Map<SavingsGoalDto>(createdGoal);

                return Ok(new BaseResponse<SavingsGoalDto>
                {
                    Success = true,
                    Data = goalResponse,
                    Message = "Savings goal created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating savings goal");
                return StatusCode(500, new BaseResponse<SavingsGoalDto>
                {
                    Success = false,
                    Message = "An error occurred while creating the savings goal"
                });
            }
        }

        [HttpGet("payees")]
        public async Task<ActionResult<BaseResponse<List<SavedPayeeDto>>>> GetSavedPayees()
        {
            try
            {
                var userId = GetCurrentUserId();
                var profile = await _clientPortalService.GetClientProfileAsync(userId);

                if (profile == null)
                {
                    return NotFound(new BaseResponse<List<SavedPayeeDto>>
                    {
                        Success = false,
                        Message = "Client profile not found"
                    });
                }

                var payees = await _clientPortalService.GetSavedPayeesAsync(profile.Id);
                var payeeDtos = _mapper.Map<List<SavedPayeeDto>>(payees);

                return Ok(new BaseResponse<List<SavedPayeeDto>>
                {
                    Success = true,
                    Data = payeeDtos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving saved payees");
                return StatusCode(500, new BaseResponse<List<SavedPayeeDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving saved payees"
                });
            }
        }

        [HttpPost("payees")]
        public async Task<ActionResult<BaseResponse<SavedPayeeDto>>> CreateSavedPayee(CreateSavedPayeeDto payeeDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var profile = await _clientPortalService.GetClientProfileAsync(userId);

                if (profile == null)
                {
                    return NotFound(new BaseResponse<SavedPayeeDto>
                    {
                        Success = false,
                        Message = "Client profile not found"
                    });
                }

                var payee = _mapper.Map<SavedPayee>(payeeDto);
                payee.ClientPortalProfileId = profile.Id;

                var createdPayee = await _clientPortalService.CreateSavedPayeeAsync(payee);
                var payeeResponse = _mapper.Map<SavedPayeeDto>(createdPayee);

                return Ok(new BaseResponse<SavedPayeeDto>
                {
                    Success = true,
                    Data = payeeResponse,
                    Message = "Payee created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating saved payee");
                return StatusCode(500, new BaseResponse<SavedPayeeDto>
                {
                    Success = false,
                    Message = "An error occurred while creating the payee"
                });
            }
        }

        [HttpDelete("payees/{payeeId}")]
        public async Task<ActionResult<BaseResponse<bool>>> DeleteSavedPayee(Guid payeeId)
        {
            try
            {
                var result = await _clientPortalService.DeleteSavedPayeeAsync(payeeId);

                if (!result)
                {
                    return NotFound(new BaseResponse<bool>
                    {
                        Success = false,
                        Message = "Payee not found"
                    });
                }

                return Ok(new BaseResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Payee deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting saved payee");
                return StatusCode(500, new BaseResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while deleting the payee"
                });
            }
        }

        #region Helper Methods

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            
            return Guid.Parse(userIdClaim);
        }

        #endregion
    }
}
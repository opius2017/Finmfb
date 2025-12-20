using System;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.Features.ClientPortal.Commands.CreateActivity
{
    public class CreateClientPortalActivityCommand : IRequest<string>
    {
        public string ClientPortalProfileId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        [Required]
        public string ActivityType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? AdditionalData { get; set; }
    }
}

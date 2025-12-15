using System;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.Features.ClientPortal.Commands.CreateActivity
{
    public class CreateClientPortalActivityCommand : IRequest<Guid>
    {
        public Guid ClientPortalProfileId { get; set; }
        public Guid UserId { get; set; }
        public Guid SessionId { get; set; }
        [Required]
        public string ActivityType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? AdditionalData { get; set; }
    }
}

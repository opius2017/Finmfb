using System;
using System.Threading;
using System.Threading.Tasks;
using FinTech.Core.Application.Common.Interfaces;
using FinTech.Core.Domain.Entities.ClientPortal;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinTech.Core.Application.Features.ClientPortal.Commands.CreateActivity
{
    public class CreateClientPortalActivityCommandHandler : IRequestHandler<CreateClientPortalActivityCommand, string>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<CreateClientPortalActivityCommandHandler> _logger;

        public CreateClientPortalActivityCommandHandler(IApplicationDbContext context, ILogger<CreateClientPortalActivityCommandHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string> Handle(CreateClientPortalActivityCommand request, CancellationToken cancellationToken)
        {
            var activity = ClientPortalActivity.Create(
                request.ClientPortalProfileId,
                request.UserId,
                request.SessionId,
                request.ActivityType,
                request.Description,
                request.IpAddress ?? "0.0.0.0",
                request.UserAgent ?? "Unknown",
                request.AdditionalData
            );

            _context.ClientPortalActivities.Add(activity);
            
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Client Portal Activity created with ID: {ActivityId}", activity.Id);

            return activity.Id;
        }
    }
}

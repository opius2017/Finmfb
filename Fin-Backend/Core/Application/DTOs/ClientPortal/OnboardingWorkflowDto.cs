using System;
using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    public class OnboardingWorkflowDto
    {
        public Guid WorkflowId { get; set; }
        public Guid CustomerId { get; set; }
        public string Status { get; set; } = "Initiated";
        public DateTime StartedAt { get; set; }
        public List<string> Steps { get; set; } = new();
    }
}

using System;
using System.Collections.Generic;

namespace FinTech.Application.DTOs.ClientPortal
{
    public class OnboardingRequestDto
    {
        public Guid CustomerId { get; set; }
        public string Channel { get; set; } = "Web";
        public string InitiatedBy { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.DTOs.ClientPortal
{
    // Client Session DTOs
    public class ClientSessionDto
    {
        public Guid Id { get; set; }
        public string SessionId { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string DeviceInfo { get; set; }
        public string Location { get; set; }
        public bool IsActive { get; set; }
    }
}
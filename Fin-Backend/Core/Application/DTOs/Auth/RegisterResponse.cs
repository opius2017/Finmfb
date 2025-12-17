using System;

namespace FinTech.Core.Application.DTOs.Auth
{
    public class RegisterResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}

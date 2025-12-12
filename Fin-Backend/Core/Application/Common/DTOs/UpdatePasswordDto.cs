using System.ComponentModel.DataAnnotations;

namespace FinTech.Core.Application.Common.DTOs
{
    public class UpdatePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public string NewPassword { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs
{
    public class ForgotPasswordRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}

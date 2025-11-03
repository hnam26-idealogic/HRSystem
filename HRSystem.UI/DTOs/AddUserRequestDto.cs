using System.ComponentModel.DataAnnotations;

namespace HRSystem.UI.DTOs
{
    public class AddUserRequestDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Fullname { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        [Required]
        public string UserType { get; set; } = string.Empty;
        public string? AccessLevel { get; set; }
        public string? Specialty { get; set; }
    }
}
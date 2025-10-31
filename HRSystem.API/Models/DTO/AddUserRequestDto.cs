using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.Models.DTO
{
    public class AddUserRequestDto
    {
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
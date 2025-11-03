using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.Models.DTO
{
    public class AddUserRequestDto
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, ErrorMessage = "Username must be less than 50 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, ErrorMessage = "Full name must be less than 100 characters.")]
        public string Fullname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(100, ErrorMessage = "Email must be less than 100 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        [StringLength(20, ErrorMessage = "Phone number must be less than 20 characters.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "User type is required.")]
        public string UserType { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Specialty must be less than 100 characters.")]
        public string? Specialty { get; set; }

        [StringLength(50, ErrorMessage = "Access level must be less than 50 characters.")]
        public string? AccessLevel { get; set; }
    }
}
using HRSystem.API.Models.Domain;
using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.Models.DTO
{
    public class RegisterRequestDto
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        public string Fullname { get; set; }
        [Required]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        [Required]
        public UserType UserType { get; set; }
        public string AccessLevel { get; set; }
        public string Specialty { get; set; } = string.Empty;
    }

}

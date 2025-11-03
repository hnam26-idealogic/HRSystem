using System.ComponentModel.DataAnnotations;

namespace HRSystem.UI.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Username{ get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}

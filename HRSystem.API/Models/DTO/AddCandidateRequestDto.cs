using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.Models.DTO
{
    public class AddCandidateRequestDto
    {
        [Required]
        public string Fullname { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Phone { get; set; } = string.Empty;
        [Required]
        public string ResumePath { get; set; } = string.Empty;
    }
}

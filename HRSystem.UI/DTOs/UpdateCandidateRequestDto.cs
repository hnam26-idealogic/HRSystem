using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HRSystem.UI.DTOs
{
    public class UpdateCandidateRequestDto
    {
        [Required]
        public string Fullname { get; set; } = string.Empty;
        [Required]
        public string Phone { get; set; } = string.Empty;
        // public IFormFile? Resume { get; set; }
    }
}

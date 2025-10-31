using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRSystem.API.Models.Domain
{
    public class User : IdentityUser<Guid>
    {
        [Required]
        public string Fullname { get; set; } = string.Empty;

        [Required]
        public UserType UserType { get; set; }

        // HR-specific
        public string? AccessLevel { get; set; }

        // Interviewer-specific
        public string? Specialty { get; set; }

        // Navigation properties
        [NotMapped]
        public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
    }
}

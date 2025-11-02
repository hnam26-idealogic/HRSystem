using System.ComponentModel.DataAnnotations.Schema;

namespace HRSystem.API.Models.Domain
{
    public class Candidate : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Fullname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        [NotMapped]
        public IFormFile Resume { get; set; } = null!;
        public string ResumePath { get; set; } = string.Empty;
        public ICollection<Interview> Interviews { get; set; } = null!;
    }
}

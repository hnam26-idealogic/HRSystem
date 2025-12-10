using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRSystem.API.Models.Domain
{
    public class Interview : BaseEntity
    {
        public Guid Id { get; set; }

        [Required]
        public string Job { get; set; } = string.Empty;

        [Required]
        public Guid CandidateId { get; set; }
        
        [ForeignKey(nameof(CandidateId))]
        public Candidate Candidate { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string InterviewerEmail { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string HrEmail { get; set; } = string.Empty;

        public DateTime InterviewedAt { get; set; }
        [NotMapped]
        public IFormFile RecordingFile { get; set; }
        public string? Recording { get; set; } = string.Empty;
        public int English { get; set; }
        public int Technical { get; set; }
        public int Recommend { get; set; }

        public string Status { get; set; } = "Scheduled";
    }
}

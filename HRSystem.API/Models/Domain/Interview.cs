using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.Models.Domain
{
    public class Interview : BaseEntity
    {
        public Guid Id { get; set; }

        [Required]
        public string Job { get; set; } = string.Empty;

        [Required]
        public Guid CandidateId { get; set; }
        public Candidate Candidate { get; set; } = null!;

        [Required]
        public Guid InterviewerId { get; set; }
        public User Interviewer { get; set; } = null!; // Interviewer is a User

        [Required]
        public Guid HrId { get; set; }
        public User HR { get; set; } = null!; // HR is a User

        public DateTime InterviewedAt { get; set; }
        public byte[]? Recording { get; set; }
        public int English { get; set; }
        public int Technical { get; set; }
        public int Recommend { get; set; }

        public string Status { get; set; } = "Scheduled";
    }
}

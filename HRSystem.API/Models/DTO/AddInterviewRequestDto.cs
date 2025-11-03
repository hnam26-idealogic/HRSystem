using System;
using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.Models.DTO
{
    public class AddInterviewRequestDto
    {
        [Required]
        [StringLength(100)]
        public string Job { get; set; } = string.Empty;

        [Required]
        public Guid CandidateId { get; set; }

        [Required]
        public Guid InterviewerId { get; set; }

        [Required]
        public Guid HrId { get; set; }

        [Required]
        public DateTime InterviewedAt { get; set; }

        [StringLength(500)]
        public string Recording { get; set; } = string.Empty;

        [Range(1, 5)]
        public int English { get; set; }

        [Range(1, 5)]
        public int Technical { get; set; }

        [Range(1, 5)]
        public int Recommend { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Scheduled";
    }
}

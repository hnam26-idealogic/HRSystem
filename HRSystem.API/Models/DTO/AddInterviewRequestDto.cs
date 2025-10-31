using System;
using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.Models.DTO
{
    public class AddInterviewRequestDto
    {
        [Required]
        public string Job { get; set; } = string.Empty;

        [Required]
        public Guid CandidateId { get; set; }

        [Required]
        public Guid InterviewerId { get; set; }

        [Required]
        public Guid HrId { get; set; }

        [Required]
        public DateTime InterviewedAt { get; set; }

        public byte[]? Recording { get; set; }
        public int English { get; set; }
        public int Technical { get; set; }
        public int Recommend { get; set; }
        public string Status { get; set; } = "Scheduled";
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace HRSystem.UI.DTOs
{
    public class AddInterviewRequestDto
    {
        [Required(ErrorMessage = "Job is required.")]
        [StringLength(100, ErrorMessage = "Job title must be less than 100 characters.")]
        public string Job { get; set; } = string.Empty;

        [Required(ErrorMessage = "Candidate is required.")]
        public Guid CandidateId { get; set; }

        [Required(ErrorMessage = "Interviewer is required.")]
        public Guid InterviewerId { get; set; }

        [Required(ErrorMessage = "HR is required.")]
        public Guid HrId { get; set; }

        [Required(ErrorMessage = "Interview date is required.")]
        public DateTime InterviewedAt { get; set; }

        [StringLength(500, ErrorMessage = "Recording path must be less than 500 characters.")]
        public string Recording { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "English score must be between 1 and 5.")]
        public int English { get; set; }

        [Range(1, 5, ErrorMessage = "Technical score must be between 1 and 5.")]
        public int Technical { get; set; }

        [Range(1, 5, ErrorMessage = "Recommend score must be between 1 and 5.")]
        public int Recommend { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(50, ErrorMessage = "Status must be less than 50 characters.")]
        public string Status { get; set; } = "Scheduled";
    }
}

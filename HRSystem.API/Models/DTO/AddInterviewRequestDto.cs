using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

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

        [Range(1, 5, ErrorMessage = "English score must be between 1 and 5.")]
        public int? English { get; set; } = null;

        [Range(1, 5, ErrorMessage = "Technical score must be between 1 and 5.")]
        public int? Technical { get; set; } = null;

        [Range(1, 5, ErrorMessage = "Recommend score must be between 1 and 5.")]
        public int? Recommend { get; set; } = null;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Scheduled";

        public IFormFile? Recording { get; set; }
    }
}

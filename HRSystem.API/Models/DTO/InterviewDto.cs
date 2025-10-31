using System;

namespace HRSystem.API.Models.DTO
{
    public class InterviewDto
    {
        public Guid Id { get; set; }
        public string Job { get; set; } = string.Empty;
        public Guid CandidateId { get; set; }
        public Guid InterviewerId { get; set; }
        public Guid HrId { get; set; }
        public DateTime InterviewedAt { get; set; }
        public int English { get; set; }
        public int Technical { get; set; }
        public int Recommend { get; set; }
        public string Status { get; set; } = "Scheduled";
    }
}

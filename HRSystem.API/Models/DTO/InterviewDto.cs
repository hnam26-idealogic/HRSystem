using System;

namespace HRSystem.API.Models.DTO
{
    public class InterviewDto
    {
        public Guid Id { get; set; }
        public string Job { get; set; } = string.Empty;
        public Guid CandidateId { get; set; }
        public string CandidateName { get; set; }
        public Guid InterviewerId { get; set; }
        public string InterviewerName { get; set; }
        public Guid HrId { get; set; }
        public string HRName { get; set; }
        public DateTime InterviewedAt { get; set; }
        public string Recording { get; set; }
        public int English { get; set; }
        public int Technical { get; set; }
        public int Recommend { get; set; }
        public string Status { get; set; } = "Scheduled";
    }
}

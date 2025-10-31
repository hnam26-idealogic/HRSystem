using System;

namespace HRSystem.API.Models.DTO
{
    public class CandidateDto
    {
        public Guid Id { get; set; }
        public string Fullname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ResumePath { get; set; } = string.Empty;
    }
}

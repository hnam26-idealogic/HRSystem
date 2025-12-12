using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;

namespace HRSystem.API.Models.Search
{
    public class CandidateSearchDocument
    {
        [SearchableField(IsKey = true, IsFilterable = true)]
        public string Id { get; set; } = string.Empty;

        [SearchableField(IsSortable = true, AnalyzerName = LexicalAnalyzerName.Values.StandardLucene)]
        public string Fullname { get; set; } = string.Empty;

        [SearchableField(IsFilterable = true, IsSortable = true)]
        public string Email { get; set; } = string.Empty;

        [SearchableField]
        public string Phone { get; set; } = string.Empty;

        [SearchableField(AnalyzerName = LexicalAnalyzerName.Values.StandardLucene)]
        public string? ResumeContent { get; set; }

        [SimpleField(IsFilterable = true)]
        public string ResumePath { get; set; } = string.Empty;

        [SimpleField(IsFilterable = true, IsSortable = true)]
        public DateTime CreatedAt { get; set; }

        [SimpleField(IsFilterable = true)]
        public bool IsDeleted { get; set; }
    }
}

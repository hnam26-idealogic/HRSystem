using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using HRSystem.API.Models.Domain;
using HRSystem.API.Models.Search;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HRSystem.API.Services
{
    public class AzureSearchService : IAzureSearchService
    {
        private readonly SearchClient _searchClient;
        private readonly SearchIndexClient _indexClient;
        private readonly ILogger<AzureSearchService> _logger;
        private readonly IResumeExtractionService _resumeExtractionService;
        private const string IndexName = "candidates-index";

        public AzureSearchService(
            IConfiguration configuration,
            ILogger<AzureSearchService> logger,
            IResumeExtractionService resumeExtractionService)
        {
            _logger = logger;
            _resumeExtractionService = resumeExtractionService;

            var searchServiceEndpoint = configuration["AzureSearch:ServiceEndpoint"];
            var searchApiKey = configuration["AzureSearch:ApiKey"];

            if (string.IsNullOrEmpty(searchServiceEndpoint) || string.IsNullOrEmpty(searchApiKey))
            {
                _logger.LogWarning("Azure Search configuration missing. Search functionality will be limited.");
                // Fallback mode - will use SQL search
                return;
            }

            var credential = new AzureKeyCredential(searchApiKey);
            _indexClient = new SearchIndexClient(new Uri(searchServiceEndpoint), credential);
            _searchClient = _indexClient.GetSearchClient(IndexName);

            _logger.LogInformation("Azure Search Service initialized with index: {IndexName}", IndexName);
        }

        public async Task<(IEnumerable<Candidate> Items, int TotalCount)> SearchCandidatesAsync(string query, int page = 1, int size = 10)
        {
            try
            {
                if (_searchClient == null)
                {
                    _logger.LogWarning("Azure Search client not initialized. Cannot perform search.");
                    return (Enumerable.Empty<Candidate>(), 0);
                }

                _logger.LogInformation("Searching candidates with Azure Search. Query: '{Query}', Page: {Page}, Size: {Size}", 
                    query, page, size);

                var searchOptions = new SearchOptions
                {
                    Filter = "IsDeleted eq false",
                    Size = size,
                    Skip = (page - 1) * size,
                    IncludeTotalCount = true,
                    OrderBy = { "Fullname" }
                };

                // Add searchable fields with boosting
                searchOptions.SearchFields.Add("Fullname");
                searchOptions.SearchFields.Add("Email");
                searchOptions.SearchFields.Add("ResumeContent");

                // If query is empty, get all results
                var searchText = string.IsNullOrWhiteSpace(query) ? "*" : query;

                var response = await _searchClient.SearchAsync<CandidateSearchDocument>(searchText, searchOptions);
                
                var candidates = new List<Candidate>();
                await foreach (var result in response.Value.GetResultsAsync())
                {
                    var doc = result.Document;
                    candidates.Add(new Candidate
                    {
                        Id = Guid.Parse(doc.Id),
                        Fullname = doc.Fullname,
                        Email = doc.Email,
                        Phone = doc.Phone,
                        ResumePath = doc.ResumePath,
                        CreatedAt = doc.CreatedAt
                    });
                }

                var totalCount = (int)(response.Value.TotalCount ?? 0);
                _logger.LogInformation("Azure Search completed. Query: '{Query}', Found: {Count} candidates", query, totalCount);

                return (candidates, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing Azure Search. Query: '{Query}'", query);
                throw;
            }
        }

        public async Task IndexCandidateAsync(Candidate candidate)
        {
            try
            {
                if (_searchClient == null)
                {
                    _logger.LogWarning("Azure Search client not initialized. Skipping indexing.");
                    return;
                }

                _logger.LogInformation("Indexing candidate: {CandidateId}, Name: {Name}", candidate.Id, candidate.Fullname);

                // Extract text from resume if available
                string? resumeContent = null;
                if (!string.IsNullOrEmpty(candidate.ResumePath))
                {
                    resumeContent = await _resumeExtractionService.ExtractTextFromResumeAsync(candidate.ResumePath);
                }

                var document = new CandidateSearchDocument
                {
                    Id = candidate.Id.ToString(),
                    Fullname = candidate.Fullname,
                    Email = candidate.Email,
                    Phone = candidate.Phone,
                    ResumePath = candidate.ResumePath,
                    ResumeContent = resumeContent,
                    CreatedAt = candidate.CreatedAt,
                    IsDeleted = candidate.DeletedAt.HasValue
                };

                await _searchClient.IndexDocumentsAsync(IndexDocumentsBatch.Upload(new[] { document }));
                _logger.LogInformation("Successfully indexed candidate: {CandidateId}", candidate.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing candidate: {CandidateId}", candidate.Id);
                // Don't throw - indexing failure shouldn't break the main flow
            }
        }

        public async Task UpdateCandidateIndexAsync(Candidate candidate)
        {
            try
            {
                if (_searchClient == null)
                {
                    _logger.LogWarning("Azure Search client not initialized. Skipping update.");
                    return;
                }

                _logger.LogInformation("Updating candidate index: {CandidateId}", candidate.Id);

                // Extract text from resume if available
                string? resumeContent = null;
                if (!string.IsNullOrEmpty(candidate.ResumePath))
                {
                    resumeContent = await _resumeExtractionService.ExtractTextFromResumeAsync(candidate.ResumePath);
                }

                var document = new CandidateSearchDocument
                {
                    Id = candidate.Id.ToString(),
                    Fullname = candidate.Fullname,
                    Email = candidate.Email,
                    Phone = candidate.Phone,
                    ResumePath = candidate.ResumePath,
                    ResumeContent = resumeContent,
                    CreatedAt = candidate.CreatedAt,
                    IsDeleted = candidate.DeletedAt.HasValue
                };

                await _searchClient.MergeOrUploadDocumentsAsync(new[] { document });
                _logger.LogInformation("Successfully updated candidate index: {CandidateId}", candidate.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating candidate index: {CandidateId}", candidate.Id);
                // Don't throw - indexing failure shouldn't break the main flow
            }
        }

        public async Task DeleteCandidateFromIndexAsync(Guid candidateId)
        {
            try
            {
                if (_searchClient == null)
                {
                    _logger.LogWarning("Azure Search client not initialized. Skipping deletion.");
                    return;
                }

                _logger.LogInformation("Deleting candidate from index: {CandidateId}", candidateId);

                var document = new CandidateSearchDocument { Id = candidateId.ToString() };
                await _searchClient.DeleteDocumentsAsync(new[] { document });

                _logger.LogInformation("Successfully deleted candidate from index: {CandidateId}", candidateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting candidate from index: {CandidateId}", candidateId);
                // Don't throw - indexing failure shouldn't break the main flow
            }
        }
    }
}

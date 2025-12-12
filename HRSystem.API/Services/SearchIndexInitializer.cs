using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HRSystem.API.Services
{
    public class SearchIndexInitializer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SearchIndexInitializer> _logger;

        public SearchIndexInitializer(IConfiguration configuration, ILogger<SearchIndexInitializer> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InitializeIndexAsync()
        {
            try
            {
                var searchServiceEndpoint = _configuration["AzureSearch:ServiceEndpoint"];
                var searchApiKey = _configuration["AzureSearch:ApiKey"];
                var indexName = _configuration["AzureSearch:IndexName"] ?? "candidates-index";

                if (string.IsNullOrEmpty(searchServiceEndpoint) || string.IsNullOrEmpty(searchApiKey))
                {
                    _logger.LogWarning("Azure Search configuration missing. Skipping index initialization.");
                    return;
                }

                var credential = new AzureKeyCredential(searchApiKey);
                var indexClient = new SearchIndexClient(new Uri(searchServiceEndpoint), credential);

                _logger.LogInformation("Checking if Azure Search index '{IndexName}' exists...", indexName);

                // Check if index exists
                try
                {
                    await indexClient.GetIndexAsync(indexName);
                    _logger.LogInformation("Index '{IndexName}' already exists.", indexName);
                    return;
                }
                catch (RequestFailedException ex) when (ex.Status == 404)
                {
                    _logger.LogInformation("Index '{IndexName}' does not exist. Creating...", indexName);
                }

                // Define the index schema
                var definition = new SearchIndex(indexName)
                {
                    Fields =
                    {
                        new SearchableField("Fullname") 
                        { 
                            IsSortable = true,
                            AnalyzerName = LexicalAnalyzerName.StandardLucene
                        },
                        new SearchableField("Email") 
                        { 
                            IsFilterable = true,
                            IsSortable = true
                        },
                        new SearchableField("Phone"),
                        new SearchableField("ResumeContent")
                        {
                            AnalyzerName = LexicalAnalyzerName.StandardLucene
                        },
                        new SimpleField("Id", SearchFieldDataType.String) 
                        { 
                            IsKey = true,
                            IsFilterable = true
                        },
                        new SimpleField("ResumePath", SearchFieldDataType.String)
                        {
                            IsFilterable = true
                        },
                        new SimpleField("CreatedAt", SearchFieldDataType.DateTimeOffset)
                        {
                            IsFilterable = true,
                            IsSortable = true
                        },
                        new SimpleField("IsDeleted", SearchFieldDataType.Boolean)
                        {
                            IsFilterable = true
                        }
                    }
                };

                // Create the index
                await indexClient.CreateIndexAsync(definition);
                _logger.LogInformation("✓ Successfully created index '{IndexName}'", indexName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Azure Search index. The application will continue but search functionality may be limited.");
            }
        }
    }
}

using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace HRSystem.API.Services
{
    public class ResumeExtractionService : IResumeExtractionService
    {
        private readonly BlobServiceClient? _blobServiceClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ResumeExtractionService> _logger;

        public ResumeExtractionService(
            BlobServiceClient blobServiceClient,
            IConfiguration configuration,
            ILogger<ResumeExtractionService> logger)
        {
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string?> ExtractTextFromResumeAsync(string resumePath)
        {
            try
            {
                if (_blobServiceClient == null)
                {
                    _logger.LogWarning("BlobServiceClient not available. Cannot extract resume text.");
                    return null;
                }

                _logger.LogInformation("Extracting text from resume: {ResumePath}", resumePath);

                var containerName = _configuration["AzureBlob:ResumeContainer"];
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(resumePath);

                if (!await blobClient.ExistsAsync())
                {
                    _logger.LogWarning("Resume blob not found: {ResumePath}", resumePath);
                    return null;
                }

                // Download blob to memory
                using var memoryStream = new MemoryStream();
                await blobClient.DownloadToAsync(memoryStream);
                memoryStream.Position = 0;

                // Extract text based on file extension
                var fileExtension = Path.GetExtension(resumePath).ToLowerInvariant();
                string extractedText;

                if (fileExtension == ".pdf")
                {
                    extractedText = ExtractTextFromPdf(memoryStream);
                }
                else if (fileExtension == ".txt")
                {
                    using var reader = new StreamReader(memoryStream, Encoding.UTF8);
                    extractedText = await reader.ReadToEndAsync();
                }
                else
                {
                    _logger.LogWarning("Unsupported file format for text extraction: {Extension}", fileExtension);
                    return null;
                }

                _logger.LogInformation("Successfully extracted {CharCount} characters from resume: {ResumePath}", 
                    extractedText.Length, resumePath);

                return extractedText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from resume: {ResumePath}", resumePath);
                return null;
            }
        }

        private string ExtractTextFromPdf(Stream pdfStream)
        {
            try
            {
                using var document = PdfDocument.Open(pdfStream);
                var textBuilder = new StringBuilder();

                foreach (Page page in document.GetPages())
                {
                    var text = page.Text;
                    textBuilder.AppendLine(text);
                }

                return textBuilder.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing PDF document");
                return string.Empty;
            }
        }
    }
}

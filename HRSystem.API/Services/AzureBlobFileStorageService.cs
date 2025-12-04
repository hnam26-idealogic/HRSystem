using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace HRSystem.API.Services
{
    public class AzureBlobFileStorageService : IFileStorageService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<AzureBlobFileStorageService> _logger;

        public AzureBlobFileStorageService(
            BlobServiceClient blobServiceClient,
            IConfiguration configuration,
            ILogger<AzureBlobFileStorageService> logger)
        {
            _logger = logger;
            var containerName = configuration["AzureBlob:ResumeContainer"];
            _logger.LogInformation("Initializing Azure Blob Storage with container: {ContainerName}", containerName);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        }

        public string GetBlobSasUrl(string blobName, int expiryMinutes = 10)
        {
            try
            {
                _logger.LogInformation("Generating SAS token for blob: {BlobName}, Expiry: {ExpiryMinutes} minutes", 
                    blobName, expiryMinutes);

                var blobClient = _containerClient.GetBlobClient(blobName);

                if (!blobClient.CanGenerateSasUri)
                {
                    _logger.LogWarning("Cannot generate SAS URI for blob: {BlobName}", blobName);
                    return blobClient.Uri.ToString();
                }

                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _containerClient.Name,
                    BlobName = blobName,
                    Resource = "b",
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);
                var sasUri = blobClient.GenerateSasUri(sasBuilder);
                
                _logger.LogInformation("SAS URL generated successfully for blob: {BlobName}", blobName);
                return sasUri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating SAS URL for blob: {BlobName}", blobName);
                throw;
            }
        }

        public async Task<(string resumePath, byte[] resumeBytes)> UploadAsync(IFormFile file)
        {
            try
            {
                _logger.LogInformation("Starting Azure Blob upload. FileName: {FileName}, Size: {Size} bytes", 
                    file?.FileName, file?.Length);

                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Upload attempt with null or empty file");
                    throw new ArgumentException("No file uploaded.");
                }

                if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Invalid file type uploaded: {FileName}, ContentType: {ContentType}", 
                        file.FileName, file.ContentType);
                    throw new InvalidOperationException("Only PDF files are allowed.");
                }

                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var blobClient = _containerClient.GetBlobClient(fileName);

                _logger.LogInformation("Uploading to blob: {BlobName}", fileName);

                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
                }

                _logger.LogInformation("Blob uploaded successfully: {BlobName}", fileName);

                byte[] resumeBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    resumeBytes = memoryStream.ToArray();
                }

                _logger.LogInformation("Upload completed. BlobName: {BlobName}, BytesCount: {Count}", 
                    fileName, resumeBytes.Length);

                return (fileName, resumeBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to Azure Blob: {FileName}", file?.FileName);
                throw;
            }
        }
    }
}
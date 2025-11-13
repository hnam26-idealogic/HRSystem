using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace HRSystem.API.Services
{
    public class AzureBlobFileStorageService : IFileStorageService
    {
        private readonly BlobContainerClient containerClient;

        public AzureBlobFileStorageService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureBlobStorage");
            var containerName = configuration["AzureBlob:ResumeContainer"] ?? "resumes";
            containerClient = new BlobContainerClient(connectionString, containerName);
            containerClient.CreateIfNotExists();
        }

        public async Task<(string resumePath, byte[] resumeBytes)> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded.");

            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only PDF files are allowed.");

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            byte[] resumeBytes;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                resumeBytes = memoryStream.ToArray();
            }

            // Blob URL (public or SAS if needed)
            var urlFilePath = blobClient.Uri.ToString();

            return (urlFilePath, resumeBytes);
        }
    }
}
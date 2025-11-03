using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;

namespace HRSystem.API.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;

        public LocalFileStorageService(IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
        }

        // Uploads the file and returns (resumePath, resumeBytes)
        public async Task<(string resumePath, byte[] resumeBytes)> UploadAsync(
            IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file uploaded.");

            //if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ||
            //    file.ContentType != "application/pdf")
            //    throw new InvalidOperationException("Only PDF files are allowed.");

            var uploadsFolder = Path.Combine(webHostEnvironment.ContentRootPath,
                "Resumes");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var localFilePath = Path.Combine(uploadsFolder, fileName);

            // Save file to disk
            using (var stream = new FileStream(localFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Build the URL for accessing the file
            var urlFilePath = $"{httpContextAccessor.HttpContext.Request.Scheme}" +
                $"://{httpContextAccessor.HttpContext.Request.Host}" +
                $"{httpContextAccessor.HttpContext.Request.PathBase}" +
                $"/Resumes/{fileName}";

            // Read file bytes for storing in Candidate.Resume
            byte[] resumeBytes;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                resumeBytes = memoryStream.ToArray();
            }

            return (urlFilePath, resumeBytes);
        }
    }
}

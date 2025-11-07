using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;

namespace HRSystem.API.Services
{
    public class LocalRecordingStorageService : IRecordingStorageService
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;

        public LocalRecordingStorageService(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<(string recordingPath, byte[] recordingBytes)> UploadAsync(IFormFile file)
        {
            if (file == null)
                throw new ArgumentException("No file uploaded.");

            // Accept common audio/video types (adjust as needed)
            var allowedTypes = new[] { "audio/mpeg", "audio/wav", "audio/x-wav", "audio/mp4", "audio/x-m4a", "video/mp4", "audio/webm", "video/webm" };
            if (!allowedTypes.Contains(file.ContentType))
                throw new InvalidOperationException("Only audio/video files are allowed for recording upload.");

            var uploadsFolder = Path.Combine(webHostEnvironment.ContentRootPath, "Recordings");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var localFilePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(localFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            byte[] recordingBytes;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                recordingBytes = memoryStream.ToArray();
            }

            var urlFilePath = $"{httpContextAccessor.HttpContext.Request.Scheme}://" +
                $"{httpContextAccessor.HttpContext.Request.Host}" +
                $"{httpContextAccessor.HttpContext.Request.PathBase}/Recordings/{fileName}";

            return (urlFilePath, recordingBytes);
        }
    }
}

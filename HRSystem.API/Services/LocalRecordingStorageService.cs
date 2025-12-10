using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace HRSystem.API.Services
{
    public class LocalRecordingStorageService : IRecordingStorageService
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<LocalRecordingStorageService> _logger;

        public LocalRecordingStorageService(IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor,
            ILogger<LocalRecordingStorageService> logger)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<(string recordingPath, byte[] recordingBytes)> UploadAsync(IFormFile file)
        {
            try
            {
                _logger.LogInformation("Starting recording upload. FileName: {FileName}, Size: {Size} bytes, ContentType: {ContentType}",
                    file?.FileName, file?.Length, file?.ContentType);

                if (file == null)
                {
                    _logger.LogWarning("Upload attempt with null file");
                    throw new ArgumentException("No file uploaded.");
                }

                // Accept common audio/video types (adjust as needed)
                var allowedTypes = new[] { "audio/mpeg", "audio/wav", "audio/x-wav", "audio/mp4", "audio/x-m4a", "video/mp4", "audio/webm", "video/webm" };
                if (!allowedTypes.Contains(file.ContentType))
                {
                    _logger.LogWarning("Invalid recording file type: {FileName}, ContentType: {ContentType}",
                        file.FileName, file.ContentType);
                    throw new InvalidOperationException("Only audio/video files are allowed for recording upload.");
                }

                var uploadsFolder = Path.Combine(webHostEnvironment.ContentRootPath, "Recordings");
                Directory.CreateDirectory(uploadsFolder);
                _logger.LogDebug("Recording upload folder path: {FolderPath}", uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var localFilePath = Path.Combine(uploadsFolder, fileName);

                _logger.LogInformation("Saving recording to disk: {FilePath}", localFilePath);

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

                _logger.LogInformation("Recording uploaded successfully. FileName: {FileName}, URL: {Url}, BytesCount: {Count}",
                    fileName, urlFilePath, recordingBytes.Length);

                return (urlFilePath, recordingBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading recording: {FileName}", file?.FileName);
                throw;
            }
        }
    }
}

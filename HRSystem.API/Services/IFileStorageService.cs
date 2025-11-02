namespace HRSystem.API.Services
{
    public interface IFileStorageService
    {
        Task<(string resumePath, byte[] resumeBytes)> UploadAsync(IFormFile file);
        // Task<Stream> DownloadAsync(string filePath);
        // Task DeleteAsync(string filePath);
    }
}

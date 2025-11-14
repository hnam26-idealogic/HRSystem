namespace HRSystem.API.Services
{
    public interface IFileStorageService
    {
        string GetBlobSasUrl(string blobName, int expiryMinutes = 10);
        Task<(string resumePath, byte[] resumeBytes)> UploadAsync(IFormFile file);
        // Task<Stream> DownloadAsync(string filePath);
        // Task DeleteAsync(string filePath);
    }
}

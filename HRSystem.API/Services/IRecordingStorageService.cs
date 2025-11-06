using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace HRSystem.API.Services
{
    public interface IRecordingStorageService
    {
        Task<(string recordingPath, byte[] recordingBytes)> UploadAsync(IFormFile file);
    }
}

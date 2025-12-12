namespace HRSystem.API.Services
{
    public interface IResumeExtractionService
    {
        Task<string?> ExtractTextFromResumeAsync(string resumePath);
    }
}

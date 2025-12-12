using HRSystem.API.Data;
using HRSystem.API.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRSystem.API.Repositories
{
   public class SQLInterviewRepository : IInterviewRepository
   {
       private readonly HRSystemDBContext _dbContext;
       private readonly ILogger<SQLInterviewRepository> _logger;

       public SQLInterviewRepository(HRSystemDBContext dbContext, ILogger<SQLInterviewRepository> logger)
       {
           _dbContext = dbContext;
           _logger = logger;
       }

       public async Task<(IEnumerable<Interview> Items, int TotalCount)> GetAllAsync(int page = 1, int size = 10)
       {
           try
           {
               _logger.LogInformation("Retrieving interviews from database. Page: {Page}, Size: {Size}", page, size);
               var query = _dbContext.Interviews;
               var totalCount = await query.CountAsync();
               var items = await query
                   .OrderBy(i => i.InterviewedAt)
                   .Skip((page - 1) * size)
                   .Take(size)
                   .ToListAsync();
               _logger.LogInformation("Retrieved {Count} interviews from database. TotalCount: {TotalCount}", items.Count, totalCount);
               return (items, totalCount);
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error retrieving interviews from database. Page: {Page}, Size: {Size}", page, size);
               throw;
           }
       }

       public async Task<Interview?> GetByIdAsync(Guid id)
       {
           try
           {
               _logger.LogInformation("Retrieving interview from database: {InterviewId}", id);
               var interview = await _dbContext.Interviews.FindAsync(id);
               if (interview == null)
               {
                   _logger.LogWarning("Interview not found in database: {InterviewId}", id);
               }
               else
               {
                   _logger.LogDebug("Interview retrieved: {InterviewId}, Candidate: {CandidateId}", id, interview.CandidateId);
               }
               return interview;
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error retrieving interview from database: {InterviewId}", id);
               throw;
           }
       }

       public async Task<Interview> AddAsync(Interview interview)
       {
           try
           {
               _logger.LogInformation("Adding new interview to database. Candidate: {CandidateId}, Job: {Job}",
                   interview.CandidateId, interview.Job);
               await _dbContext.Interviews.AddAsync(interview);
               await _dbContext.SaveChangesAsync();
               _logger.LogInformation("Interview added successfully. InterviewId: {InterviewId}, Candidate: {CandidateId}",
                   interview.Id, interview.CandidateId);
               return interview;
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error adding interview to database. Candidate: {CandidateId}, Job: {Job}",
                   interview.CandidateId, interview.Job);
               throw;
           }
       }

       public async Task<Interview> UpdateAsync(Guid id, Interview interview)
       {
           try
           {
               _logger.LogInformation("Updating interview in database: {InterviewId}", id);
               var existingInterview = await _dbContext.Interviews.FindAsync(id);
               if (existingInterview == null)
               {
                   _logger.LogWarning("Interview not found for update: {InterviewId}", id);
                   throw new KeyNotFoundException("Interview not found.");
               }

               existingInterview.Job = interview.Job;
               existingInterview.CandidateId = interview.CandidateId;
               existingInterview.InterviewedAt = interview.InterviewedAt;
               existingInterview.Recording = interview.Recording;
               existingInterview.English = interview.English;
               existingInterview.Technical = interview.Technical;
               existingInterview.Recommend = interview.Recommend;
               existingInterview.Status = interview.Status;

               _dbContext.Interviews.Update(existingInterview);
               await _dbContext.SaveChangesAsync();
               _logger.LogInformation("Interview updated successfully: {InterviewId}, Status: {Status}",
                   id, existingInterview.Status);
               return existingInterview;
           }
           catch (KeyNotFoundException)
           {
               throw; // Re-throw not found exceptions
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error updating interview in database: {InterviewId}", id);
               throw;
           }
       }

       public async Task<bool> DeleteAsync(Guid id)
       {
           try
           {
               _logger.LogInformation("Deleting interview from database: {InterviewId}", id);
               var interview = await _dbContext.Interviews.FindAsync(id);
               if (interview == null)
               {
                   _logger.LogWarning("Interview not found for deletion: {InterviewId}", id);
                   return false;
               }

               _dbContext.Interviews.Remove(interview);
               await _dbContext.SaveChangesAsync();
               _logger.LogInformation("Interview deleted successfully: {InterviewId}", id);
               return true;
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error deleting interview from database: {InterviewId}", id);
               throw;
           }
       }

       public async Task<(IEnumerable<Interview> Items, int TotalCount)> SearchAsync(string query, int page = 1, int size = 10)
       {
           try
           {
               _logger.LogInformation("Searching interviews in database. Query: '{Query}', Page: {Page}, Size: {Size}",
                   query, page, size);
               var normalizedQuery = query?.Trim().ToLower() ?? string.Empty;
               
               if (string.IsNullOrWhiteSpace(normalizedQuery))
               {
                   _logger.LogDebug("Empty search query, returning all interviews");
                   return await GetAllAsync(page, size);
               }

               // Search only by candidate name/email, job, status, and interviewer/HR emails
               // User names will be matched in the controller after fetching from Graph API
               var interviewsQuery = _dbContext.Interviews
                   .Where(i =>
                       _dbContext.Candidates.Any(c => c.Id == i.CandidateId && 
                           (c.Fullname.ToLower().Contains(normalizedQuery) || 
                            c.Email.ToLower().Contains(normalizedQuery))) ||
                       i.Job.ToLower().Contains(normalizedQuery) ||
                       i.Status.ToLower().Contains(normalizedQuery) ||
                       i.InterviewerEmail.ToLower().Contains(normalizedQuery) ||
                       i.HrEmail.ToLower().Contains(normalizedQuery)
                   );

               var totalCount = await interviewsQuery.CountAsync();
               var items = await interviewsQuery
                   .OrderByDescending(i => i.InterviewedAt)
                   .Skip((page - 1) * size)
                   .Take(size)
                   .ToListAsync();

               _logger.LogInformation("Search completed. Query: '{Query}', Found: {Count} interviews", query, totalCount);
               return (items, totalCount);
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error searching interviews in database. Query: '{Query}'", query);
               throw;
           }
       }
   }
}

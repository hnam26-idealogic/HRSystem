using HRSystem.API.Data;
using HRSystem.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Repositories
{
   public class SQLInterviewRepository : IInterviewRepository
   {
       private readonly HRSystemDBContext dbContext;

       public SQLInterviewRepository(HRSystemDBContext dbContext)
       {
           this.dbContext = dbContext;
       }

       public async Task<(IEnumerable<Interview> Items, int TotalCount)> GetAllAsync(int page = 1, int size = 10)
       {
           var query = dbContext.Interviews;
           var totalCount = await query.CountAsync();
           var items = await query
               .OrderBy(i => i.InterviewedAt)
               .Skip((page - 1) * size)
               .Take(size)
               .ToListAsync();
           return (items, totalCount);
       }

       public async Task<Interview?> GetByIdAsync(Guid id)
       {
           return await dbContext.Interviews.FindAsync(id);
       }

       public async Task<Interview> AddAsync(Interview interview)
       {
           await dbContext.Interviews.AddAsync(interview);
           await dbContext.SaveChangesAsync();
           return interview;
       }

       public async Task<Interview> UpdateAsync(Guid id, Interview interview)
       {
           var existingInterview = await dbContext.Interviews.FindAsync(id);
           if (existingInterview == null)
               throw new KeyNotFoundException("Interview not found.");

           existingInterview.Job = interview.Job;
           existingInterview.CandidateId = interview.CandidateId;
           existingInterview.InterviewedAt = interview.InterviewedAt;
           existingInterview.Recording = interview.Recording;
           existingInterview.English = interview.English;
           existingInterview.Technical = interview.Technical;
           existingInterview.Recommend = interview.Recommend;
           existingInterview.Status = interview.Status;

           dbContext.Interviews.Update(existingInterview);
           await dbContext.SaveChangesAsync();
           return existingInterview;
       }

       public async Task<bool> DeleteAsync(Guid id)
       {
           var interview = await dbContext.Interviews.FindAsync(id);
           if (interview == null)
               return false;

           dbContext.Interviews.Remove(interview);
           await dbContext.SaveChangesAsync();
           return true;
       }

       public async Task<(IEnumerable<Interview> Items, int TotalCount)> SearchAsync(string query, int page = 1, int size = 10)
       {
           var normalizedQuery = query?.Trim().ToLower() ?? string.Empty;
           
           if (string.IsNullOrWhiteSpace(normalizedQuery))
           {
               return await GetAllAsync(page, size);
           }

           // Search only by candidate name/email, job, status, and interviewer/HR emails
           // User names will be matched in the controller after fetching from Graph API
           var interviewsQuery = dbContext.Interviews
               .Where(i =>
                   dbContext.Candidates.Any(c => c.Id == i.CandidateId && 
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

           return (items, totalCount);
       }
   }
}

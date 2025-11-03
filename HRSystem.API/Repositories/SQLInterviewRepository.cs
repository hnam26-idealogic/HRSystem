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
            existingInterview.InterviewerId = interview.InterviewerId;
            existingInterview.HrId = interview.HrId;
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
    }
}

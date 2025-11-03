using HRSystem.API.Data;
using HRSystem.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Repositories
{
    public class SQLCandidateRepository : ICandidateRepository
    {
        private readonly HRSystemDBContext dbContext;

        public SQLCandidateRepository(HRSystemDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<(IEnumerable<Candidate> Items, int TotalCount)> GetAllAsync(int page = 1, int size = 10)
        {
            var query = dbContext.Candidates.Where(c => c.DeletedAt == null);
            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(c => c.Fullname)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
            return (items, totalCount);
        }

        public async Task<Candidate?> GetByIdAsync(Guid id)
        {
            return await dbContext.Candidates
                .Where(c => c.DeletedAt == null && c.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Candidate?> GetByEmailAsync(string email)
        {
            return await dbContext.Candidates
                .Where(c => c.DeletedAt == null && c.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<Candidate> AddAsync(Candidate candidate)
        {
            if (await dbContext.Candidates.AnyAsync(c => c.Email == candidate.Email))
                throw new InvalidOperationException("A candidate with this email already exists.");

            await dbContext.Candidates.AddAsync(candidate);
            await dbContext.SaveChangesAsync();
            return candidate;
        }

        public async Task<Candidate> UpdateAsync(Guid id, Candidate candidate)
        {
            var existingCandidate = await dbContext.Candidates.FindAsync(id);
            if (existingCandidate == null)
                throw new KeyNotFoundException("Candidate not found.");

            existingCandidate.Fullname = candidate.Fullname;
            existingCandidate.Phone = candidate.Phone;
            existingCandidate.ResumePath = candidate.ResumePath;

            dbContext.Candidates.Update(existingCandidate);
            await dbContext.SaveChangesAsync();
            return existingCandidate;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var candidate = await dbContext.Candidates.FindAsync(id);
            if (candidate == null)
                return false;

            candidate.DeletedAt = DateTime.UtcNow;
            dbContext.Candidates.Update(candidate);
            await dbContext.SaveChangesAsync();
            return true;
        }
    }
}

using HRSystem.API.Data;
using HRSystem.API.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HRSystem.API.Repositories
{
    public class SQLCandidateRepository : ICandidateRepository
    {
        private readonly HRSystemDBContext dbContext;
        private readonly ILogger<SQLCandidateRepository> _logger;

        public SQLCandidateRepository(HRSystemDBContext dbContext, ILogger<SQLCandidateRepository> logger)
        {
            this.dbContext = dbContext;
            _logger = logger;
        }

        public async Task<(IEnumerable<Candidate> Items, int TotalCount)> GetAllAsync(int page = 1, int size = 10)
        {
            try
            {
                _logger.LogInformation("Retrieving candidates from database. Page: {Page}, Size: {Size}", page, size);
                var query = dbContext.Candidates.Where(c => c.DeletedAt == null);
                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderBy(c => c.Fullname)
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();
                _logger.LogInformation("Retrieved {Count} candidates from database. TotalCount: {TotalCount}", items.Count, totalCount);
                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving candidates from database. Page: {Page}, Size: {Size}", page, size);
                throw;
            }
        }

        public async Task<Candidate?> GetByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Retrieving candidate from database: {CandidateId}", id);
                var candidate = await dbContext.Candidates
                    .Where(c => c.DeletedAt == null && c.Id == id)
                    .FirstOrDefaultAsync();
                if (candidate == null)
                {
                    _logger.LogWarning("Candidate not found in database: {CandidateId}", id);
                }
                else
                {
                    _logger.LogDebug("Candidate retrieved: {CandidateId}, Email: {Email}", id, candidate.Email);
                }
                return candidate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving candidate from database: {CandidateId}", id);
                throw;
            }
        }

        public async Task<Candidate?> GetByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation("Retrieving candidate by email: {Email}", email);
                var candidate = await dbContext.Candidates
                    .Where(c => c.DeletedAt == null && c.Email == email)
                    .FirstOrDefaultAsync();
                if (candidate == null)
                {
                    _logger.LogDebug("Candidate not found with email: {Email}", email);
                }
                else
                {
                    _logger.LogDebug("Candidate found with email: {Email}, Id: {CandidateId}", email, candidate.Id);
                }
                return candidate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving candidate by email: {Email}", email);
                throw;
            }
        }

        public async Task<Candidate> AddAsync(Candidate candidate)
        {
            try
            {
                _logger.LogInformation("Adding new candidate to database. Email: {Email}, Name: {Name}",
                    candidate.Email, candidate.Fullname);

                if (await dbContext.Candidates.AnyAsync(c => c.Email == candidate.Email))
                {
                    _logger.LogWarning("Duplicate candidate email detected: {Email}", candidate.Email);
                    throw new InvalidOperationException("A candidate with this email already exists.");
                }

                await dbContext.Candidates.AddAsync(candidate);
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("Candidate added successfully. CandidateId: {CandidateId}, Email: {Email}",
                    candidate.Id, candidate.Email);
                return candidate;
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding candidate to database. Email: {Email}", candidate.Email);
                throw;
            }
        }

        public async Task<Candidate> UpdateAsync(Guid id, Candidate candidate)
        {
            try
            {
                _logger.LogInformation("Updating candidate in database: {CandidateId}", id);
                var existingCandidate = await dbContext.Candidates.FindAsync(id);
                if (existingCandidate == null)
                {
                    _logger.LogWarning("Candidate not found for update: {CandidateId}", id);
                    throw new KeyNotFoundException("Candidate not found.");
                }

                existingCandidate.Fullname = candidate.Fullname;
                existingCandidate.Phone = candidate.Phone;
                existingCandidate.ResumePath = candidate.ResumePath;

                dbContext.Candidates.Update(existingCandidate);
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("Candidate updated successfully: {CandidateId}, Email: {Email}",
                    id, existingCandidate.Email);
                return existingCandidate;
            }
            catch (KeyNotFoundException)
            {
                throw; // Re-throw not found exceptions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating candidate in database: {CandidateId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Soft deleting candidate: {CandidateId}", id);
                var candidate = await dbContext.Candidates.FindAsync(id);
                if (candidate == null)
                {
                    _logger.LogWarning("Candidate not found for deletion: {CandidateId}", id);
                    return false;
                }

                candidate.DeletedAt = DateTime.UtcNow;
                dbContext.Candidates.Update(candidate);
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("Candidate soft deleted successfully: {CandidateId}, DeletedAt: {DeletedAt}",
                    id, candidate.DeletedAt);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting candidate: {CandidateId}", id);
                throw;
            }
        }

        public async Task<(IEnumerable<Candidate> Items, int TotalCount)> SearchAsync(string query, int page = 1, int size = 10)
        {
            try
            {
                _logger.LogInformation("Searching candidates in database. Query: '{Query}', Page: {Page}, Size: {Size}",
                    query, page, size);
                var normalizedQuery = query?.Trim().ToLower();
                var candidatesQuery = dbContext.Candidates
                    .Where(c => c.DeletedAt == null &&
                        (c.Fullname.ToLower().Contains(normalizedQuery) || c.Email.ToLower().Contains(normalizedQuery)));
                var totalCount = await candidatesQuery.CountAsync();
                var items = await candidatesQuery
                    .OrderBy(c => c.Fullname)
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToListAsync();
                _logger.LogInformation("Search completed. Query: '{Query}', Found: {Count} candidates", query, totalCount);
                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching candidates in database. Query: '{Query}'", query);
                throw;
            }
        }
    }
}

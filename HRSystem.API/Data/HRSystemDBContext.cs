using HRSystem.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Data
{
    public class HRSystemDBContext : DbContext
    {
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Interview> Interviews { get; set; }

        public HRSystemDBContext(DbContextOptions<HRSystemDBContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure indexes for performance
            modelBuilder.Entity<Candidate>()
                .HasIndex(c => c.Email)
                .IsUnique();
            
            // Configure soft delete global query filter for Candidate
            modelBuilder.Entity<Candidate>()
                .HasQueryFilter(c => c.DeletedAt == null);
            
            // Configure soft delete global query filter for Interview
            modelBuilder.Entity<Interview>()
                .HasQueryFilter(i => i.DeletedAt == null);
        }
    }
}

using System.Reflection;
using HRSystem.API.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace HRSystem.API.Data
{
    public class HRSystemDBContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Interview> Interviews { get; set; }

        public HRSystemDBContext(DbContextOptions<HRSystemDBContext> options) : base(options)
        {
        }

        // Handle timestamps for modified and deleted entities
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                switch (entry.State)
                {
                    case EntityState.Added or EntityState.Modified:
                        entry.Entity.ModifiedAt = DateTime.UtcNow;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.DeletedAt = DateTime.UtcNow;
                        break;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Candidate>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Interview>()
                .HasOne(i => i.HR)
                .WithMany()
                .HasForeignKey(i => i.HrId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Interview>()
                .HasOne(i => i.Interviewer)
                .WithMany()
                .HasForeignKey(i => i.InterviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Interview>()
                .HasOne(i => i.Candidate)
                .WithMany(c => c.Interviews)
                .HasForeignKey(i => i.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique index for Interview: CandidateId + Job + InterviewerId
            modelBuilder.Entity<Interview>()
                .HasIndex(i => new { i.CandidateId, i.Job, i.InterviewerId })
                .IsUnique();

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Apply a global query filter to exclude soft-deleted entities
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(HRSystemDBContext)
                        .GetMethod(nameof(SetSoftDeleteFiler), BindingFlags.NonPublic | BindingFlags.Static)?
                        .MakeGenericMethod(entityType.ClrType);

                    method?.Invoke(null, new object[] { modelBuilder });
                }
            }

            modelBuilder.Entity<IdentityRole<Guid>>().HasData(
                new IdentityRole<Guid> { Id = Guid.Parse("fa38156a-39ba-4a73-9276-55a5f8bad076"), Name = "HR", NormalizedName = "HR" },
                new IdentityRole<Guid> { Id = Guid.Parse("cbe8b8d7-7e11-4f8c-b25a-189e62dbca07"), Name = "Interviewer", NormalizedName = "INTERVIEWER" }
            );
        }

        private static void SetSoftDeleteFiler<TEntity>(ModelBuilder builder) where TEntity : BaseEntity
        {
            builder.Entity<TEntity>().HasQueryFilter(e => e.DeletedAt == null);
        }
    }
}

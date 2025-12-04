using System.Reflection;
using HRSystem.API.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace HRSystem.API.Data
{
    public class HRSystemDBContext : DbContext
    {
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Interview> Interviews { get; set; }

        public HRSystemDBContext(DbContextOptions<HRSystemDBContext> options) : base(options)
        {
        }
    }
}

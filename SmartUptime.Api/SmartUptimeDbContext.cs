using Microsoft.EntityFrameworkCore;
using SmartUptime.Api.Models;

namespace SmartUptime.Api
{
    public class SmartUptimeDbContext : DbContext
    {
        public SmartUptimeDbContext(DbContextOptions<SmartUptimeDbContext> options) : base(options) { }

        public DbSet<Site> Sites => Set<Site>();
        public DbSet<PingResult> PingResults => Set<PingResult>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Site>()
                .HasMany(s => s.PingResults)
                .WithOne(p => p.Site)
                .HasForeignKey(p => p.SiteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 
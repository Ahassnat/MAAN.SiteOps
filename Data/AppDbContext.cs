using MAAN.SiteOps.Models;
using Microsoft.EntityFrameworkCore;

namespace MAAN.SiteOps.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Governorate> Governorates => Set<Governorate>();
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectSite> ProjectSites => Set<ProjectSite>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Site>()
            .HasIndex(s => s.SiteCode)
            .IsUnique();

        modelBuilder.Entity<Project>()
            .HasIndex(p => p.ProjectCode)
            .IsUnique();

        modelBuilder.Entity<ProjectSite>()
            .HasIndex(ps => new { ps.ProjectId, ps.SiteId })
            .IsUnique();

        modelBuilder.Entity<Governorate>().HasData(
            new Governorate { Id = 1, Name = "North Gaza" },
            new Governorate { Id = 2, Name = "Gaza" },
            new Governorate { Id = 3, Name = "Deir al-Balah" },
            new Governorate { Id = 4, Name = "Khan Younis" },
            new Governorate { Id = 5, Name = "Rafah" }
        );
    }
}
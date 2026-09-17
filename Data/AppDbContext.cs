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
    public DbSet<ActivityCategory> ActivityCategories => Set<ActivityCategory>();
    public DbSet<ActivityType> ActivityTypes => Set<ActivityType>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ActivityParticipant> ActivityParticipants => Set<ActivityParticipant>();
    public DbSet<ActivityStatusHistory> ActivityStatusHistories => Set<ActivityStatusHistory>();

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

        modelBuilder.Entity<ActivityCategory>()
            .HasIndex(ac => ac.Code)
            .IsUnique();

        modelBuilder.Entity<ActivityType>()
            .HasIndex(at => at.Code)
            .IsUnique();

        modelBuilder.Entity<Activity>()
            .HasOne(a => a.ProjectSite)
            .WithMany()
            .HasForeignKey(a => a.ProjectSiteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Activity>()
            .HasOne(a => a.ActivityType)
            .WithMany(at => at.Activities)
            .HasForeignKey(a => a.ActivityTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActivityParticipant>()
            .HasIndex(ap => ap.ActivityId)
            .IsUnique();

        modelBuilder.Entity<ActivityParticipant>()
            .HasOne(ap => ap.Activity)
            .WithOne(a => a.Participant)
            .HasForeignKey<ActivityParticipant>(ap => ap.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ActivityStatusHistory>()
            .HasOne(sh => sh.Activity)
            .WithMany(a => a.StatusHistory)
            .HasForeignKey(sh => sh.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Governorate>().HasData(
            new Governorate { Id = 1, Name = "North Gaza" },
            new Governorate { Id = 2, Name = "Gaza" },
            new Governorate { Id = 3, Name = "Deir al-Balah" },
            new Governorate { Id = 4, Name = "Khan Younis" },
            new Governorate { Id = 5, Name = "Rafah" }
        );

        modelBuilder.Entity<ActivityCategory>().HasData(
            new ActivityCategory { Id = 1, Code = "CPR", Name = "Community Participation & Representation", DisplayOrder = 1, IsActive = true },
            new ActivityCategory { Id = 2, Code = "SSCM", Name = "Site Service Coordination & Monitoring", DisplayOrder = 2, IsActive = true },
            new ActivityCategory { Id = 3, Code = "TWISFC", Name = "Two-Way Information Sharing, Feedback & Complaints", DisplayOrder = 3, IsActive = true },
            new ActivityCategory { Id = 4, Code = "SEI", Name = "Site Environment Improvements", DisplayOrder = 4, IsActive = true }
        );

        modelBuilder.Entity<ActivityType>().HasData(
            new ActivityType { Id = 1, ActivityCategoryId = 1, Code = "CAPACITY_STRENGTHENING", Name = "Capacity Strengthening for Community Members", RequiresParticipants = true, DisplayOrder = 1, IsActive = true },
            new ActivityType { Id = 2, ActivityCategoryId = 1, Code = "COMMUNITY_COMMITTEE_MEETING", Name = "Community or Committee Meeting", RequiresParticipants = true, DisplayOrder = 2, IsActive = true },
            new ActivityType { Id = 3, ActivityCategoryId = 2, Code = "SAFETY_AUDIT", Name = "Safety Audit", RequiresParticipants = false, DisplayOrder = 1, IsActive = true },
            new ActivityType { Id = 4, ActivityCategoryId = 3, Code = "ADVOCACY_REPORT", Name = "Advocacy Report", RequiresParticipants = false, DisplayOrder = 1, IsActive = true },
            new ActivityType { Id = 5, ActivityCategoryId = 3, Code = "AWARENESS_SESSION", Name = "Awareness Session", RequiresParticipants = true, DisplayOrder = 2, IsActive = true },
            new ActivityType { Id = 6, ActivityCategoryId = 2, Code = "SITE_IMPROVEMENT_PLAN", Name = "Site Improvement Plan", RequiresParticipants = false, DisplayOrder = 2, IsActive = true },
            new ActivityType { Id = 7, ActivityCategoryId = 4, Code = "SITE_IMPROVEMENTS_CARE_MAINTENANCE", Name = "Site Improvements, Care & Maintenance", RequiresParticipants = true, DisplayOrder = 1, IsActive = true },
            new ActivityType { Id = 8, ActivityCategoryId = 1, Code = "COMMUNITY_LED_ACTIVITY", Name = "Community-Led Activity", RequiresParticipants = true, DisplayOrder = 3, IsActive = true }
        );
    }
}
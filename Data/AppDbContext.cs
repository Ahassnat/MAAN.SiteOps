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
    public DbSet<ActivityProjectAttribution> ActivityProjectAttributions => Set<ActivityProjectAttribution>();
    public DbSet<Indicator> Indicators => Set<Indicator>();
    public DbSet<ProjectIndicator> ProjectIndicators => Set<ProjectIndicator>();
    public DbSet<ActivityIndicatorContribution> ActivityIndicatorContributions => Set<ActivityIndicatorContribution>();
    public DbSet<SourceSystem> SourceSystems => Set<SourceSystem>();
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<ActivitySourceRecord> ActivitySourceRecords => Set<ActivitySourceRecord>();
    public DbSet<ReportDefinition> ReportDefinitions => Set<ReportDefinition>();
    public DbSet<ProjectReport> ProjectReports => Set<ProjectReport>();
    public DbSet<ReportIndicatorMapping> ReportIndicatorMappings => Set<ReportIndicatorMapping>();

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
            .HasOne(a => a.Site)
            .WithMany(s => s.Activities)
            .HasForeignKey(a => a.SiteId)
            .OnDelete(DeleteBehavior.Restrict);

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

        modelBuilder.Entity<ActivityProjectAttribution>()
            .HasIndex(apa => new { apa.ActivityId, apa.ProjectSiteId })
            .IsUnique();

        modelBuilder.Entity<ActivityProjectAttribution>()
            .HasOne(apa => apa.Activity)
            .WithMany(a => a.ProjectAttributions)
            .HasForeignKey(apa => apa.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ActivityProjectAttribution>()
            .HasOne(apa => apa.ProjectSite)
            .WithMany(ps => ps.ActivityProjectAttributions)
            .HasForeignKey(apa => apa.ProjectSiteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProjectIndicator>()
            .HasIndex(pi => new { pi.ProjectId, pi.ProjectIndicatorCode })
            .IsUnique();

        modelBuilder.Entity<ProjectIndicator>()
            .HasOne(pi => pi.Project)
            .WithMany(p => p.ProjectIndicators)
            .HasForeignKey(pi => pi.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProjectIndicator>()
            .HasOne(pi => pi.Indicator)
            .WithMany(i => i.ProjectIndicators)
            .HasForeignKey(pi => pi.IndicatorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActivityIndicatorContribution>()
            .HasOne(aic => aic.Activity)
            .WithMany(a => a.IndicatorContributions)
            .HasForeignKey(aic => aic.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ActivityIndicatorContribution>()
            .HasOne(aic => aic.ProjectIndicator)
            .WithMany(pi => pi.ActivityIndicatorContributions)
            .HasForeignKey(aic => aic.ProjectIndicatorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SourceSystem>()
            .HasIndex(ss => ss.Code)
            .IsUnique();

        modelBuilder.Entity<ImportBatch>()
            .HasOne(ib => ib.SourceSystem)
            .WithMany(ss => ss.ImportBatches)
            .HasForeignKey(ib => ib.SourceSystemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActivitySourceRecord>()
            .HasIndex(asr => new { asr.SourceSystemId, asr.ExternalRecordId });

        modelBuilder.Entity<ActivitySourceRecord>()
            .HasOne(asr => asr.SourceSystem)
            .WithMany(ss => ss.ActivitySourceRecords)
            .HasForeignKey(asr => asr.SourceSystemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActivitySourceRecord>()
            .HasOne(asr => asr.ImportBatch)
            .WithMany(ib => ib.ActivitySourceRecords)
            .HasForeignKey(asr => asr.ImportBatchId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ActivitySourceRecord>()
            .HasOne(asr => asr.Activity)
            .WithMany(a => a.SourceRecords)
            .HasForeignKey(asr => asr.ActivityId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ReportDefinition>()
            .HasIndex(rd => rd.Code)
            .IsUnique();

        modelBuilder.Entity<ProjectReport>()
            .HasIndex(pr => new { pr.ProjectId, pr.ReportDefinitionId })
            .IsUnique();

        modelBuilder.Entity<ProjectReport>()
            .HasOne(pr => pr.Project)
            .WithMany(p => p.ProjectReports)
            .HasForeignKey(pr => pr.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProjectReport>()
            .HasOne(pr => pr.ReportDefinition)
            .WithMany(rd => rd.ProjectReports)
            .HasForeignKey(pr => pr.ReportDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReportIndicatorMapping>()
            .HasIndex(rim => new { rim.ProjectReportId, rim.ReportFieldCode })
            .IsUnique();

        modelBuilder.Entity<ReportIndicatorMapping>()
            .HasOne(rim => rim.ProjectReport)
            .WithMany(pr => pr.ReportIndicatorMappings)
            .HasForeignKey(rim => rim.ProjectReportId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ReportIndicatorMapping>()
            .HasOne(rim => rim.ProjectIndicator)
            .WithMany(pi => pi.ReportIndicatorMappings)
            .HasForeignKey(rim => rim.ProjectIndicatorId)
            .OnDelete(DeleteBehavior.Restrict);

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
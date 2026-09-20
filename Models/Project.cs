using System.ComponentModel.DataAnnotations;

namespace MAAN.SiteOps.Models;

public class Project
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string ProjectCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string ProjectName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Donor { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Planned";

    public bool IsActive { get; set; } = true;

    public ICollection<ProjectSite> ProjectSites { get; set; }
        = new List<ProjectSite>();

    public ICollection<ProjectIndicator> ProjectIndicators { get; set; }
        = new List<ProjectIndicator>();

    public ICollection<ProjectReport> ProjectReports { get; set; }
        = new List<ProjectReport>();
}
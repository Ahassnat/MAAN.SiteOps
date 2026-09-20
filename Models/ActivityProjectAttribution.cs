using System.ComponentModel.DataAnnotations;

namespace MAAN.SiteOps.Models;

public class ActivityProjectAttribution
{
    public int Id { get; set; }

    [Display(Name = "Activity")]
    public int ActivityId { get; set; }

    public Activity? Activity { get; set; }

    [Display(Name = "Project Site")]
    public int ProjectSiteId { get; set; }

    public ProjectSite? ProjectSite { get; set; }

    public bool IsPrimary { get; set; }

    [MaxLength(50)]
    public string? AttributionRole { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
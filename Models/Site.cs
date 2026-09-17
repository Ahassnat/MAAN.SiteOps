using System.ComponentModel.DataAnnotations;

namespace MAAN.SiteOps.Models;

public class Site
{
    public int Id { get; set; }

    [Required]
    [MaxLength(30)]
    [Display(Name = "Site Code")]
    public string SiteCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Display(Name = "Site Name")]
    public string SiteName { get; set; } = string.Empty;

    [Display(Name = "Governorate")]
    public int GovernorateId { get; set; }

    public Governorate? Governorate { get; set; }

    [MaxLength(150)]
    public string? Area { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Managed by MA'AN")]
    public bool IsManagedByMaan { get; set; } = true;

    [ScaffoldColumn(false)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ScaffoldColumn(false)]
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ProjectSite> ProjectSites { get; set; }
        = new List<ProjectSite>();
}
using System.ComponentModel.DataAnnotations;

namespace MAAN.SiteOps.ViewModels;

public class HistoricalSiteResolutionViewModel
{
    [Required, MaxLength(30)]
    public string SiteCode { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string SiteName { get; set; } = string.Empty;

    [Required]
    public int? GovernorateId { get; set; }

    [MaxLength(150)]
    public string? Area { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public int ActivityRecordCount { get; set; }
    public DateTime? EarliestActivityDate { get; set; }
    public DateTime? LatestActivityDate { get; set; }
}
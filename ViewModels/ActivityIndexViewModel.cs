using MAAN.SiteOps.Models;

namespace MAAN.SiteOps.ViewModels;

public class ActivityIndexViewModel
{
    public IEnumerable<Activity> Activities { get; set; } = [];

    public int? ProjectId { get; set; }

    public int? ProjectSiteId { get; set; }

    public int? SiteId { get; set; }

    public int? ActivityTypeId { get; set; }

    public string? Status { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }
}
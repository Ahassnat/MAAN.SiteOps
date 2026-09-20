using System.ComponentModel.DataAnnotations;

namespace MAAN.SiteOps.Models;

public class ActivityIndicatorContribution
{
    public int Id { get; set; }

    [Display(Name = "Activity")]
    public int ActivityId { get; set; }

    public Activity? Activity { get; set; }

    [Display(Name = "Project Indicator")]
    public int ProjectIndicatorId { get; set; }

    public ProjectIndicator? ProjectIndicator { get; set; }

    public decimal? Value { get; set; }

    public int? Men { get; set; }

    public int? Women { get; set; }

    public int? Boys { get; set; }

    public int? Girls { get; set; }

    [Display(Name = "Persons with Disabilities")]
    public int? PersonsWithDisabilities { get; set; }

    [DataType(DataType.Date)]
    public DateTime ContributionDate { get; set; }

    public bool IsManual { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
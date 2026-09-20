using System.ComponentModel.DataAnnotations;

namespace MAAN.SiteOps.Models;

public class ProjectIndicator
{
    public int Id { get; set; }

    [Display(Name = "Project")]
    public int ProjectId { get; set; }

    public Project? Project { get; set; }

    [Display(Name = "Indicator")]
    public int IndicatorId { get; set; }

    public Indicator? Indicator { get; set; }

    [Required]
    [MaxLength(100)]
    public string ProjectIndicatorCode { get; set; } = string.Empty;

    public decimal? TargetValue { get; set; }

    public int? TargetMen { get; set; }

    public int? TargetWomen { get; set; }

    public int? TargetBoys { get; set; }

    public int? TargetGirls { get; set; }

    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [MaxLength(1000)]
    public string? MeansOfVerification { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ActivityIndicatorContribution> ActivityIndicatorContributions { get; set; }
        = new List<ActivityIndicatorContribution>();

    public ICollection<ReportIndicatorMapping> ReportIndicatorMappings { get; set; }
        = new List<ReportIndicatorMapping>();
}
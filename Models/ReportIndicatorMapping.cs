using System.ComponentModel.DataAnnotations;

namespace MAAN.SiteOps.Models;

public class ReportIndicatorMapping
{
    public int Id { get; set; }
    public int ProjectReportId { get; set; }
    public ProjectReport? ProjectReport { get; set; }
    public int ProjectIndicatorId { get; set; }
    public ProjectIndicator? ProjectIndicator { get; set; }
    [Required, MaxLength(100)] public string ReportFieldCode { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string ReportFieldName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    [MaxLength(1000)] public string? Notes { get; set; }
}
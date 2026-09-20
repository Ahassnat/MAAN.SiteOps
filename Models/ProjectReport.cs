using System.ComponentModel.DataAnnotations;

namespace MAAN.SiteOps.Models;

public class ProjectReport
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
    public int ReportDefinitionId { get; set; }
    public ReportDefinition? ReportDefinition { get; set; }
    [DataType(DataType.Date)] public DateTime? StartDate { get; set; }
    [DataType(DataType.Date)] public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    [MaxLength(1000)] public string? Notes { get; set; }
    public ICollection<ReportIndicatorMapping> ReportIndicatorMappings { get; set; } = new List<ReportIndicatorMapping>();
}
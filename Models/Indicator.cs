using System.ComponentModel.DataAnnotations;

namespace MAAN.SiteOps.Models;

public class Indicator
{
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? GlobalCode { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(30)]
    public string UnitType { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string CalculationMode { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string DataSourceType { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<ProjectIndicator> ProjectIndicators { get; set; }
        = new List<ProjectIndicator>();
}
using System.ComponentModel.DataAnnotations;

namespace MAAN.SiteOps.Models;

public class ActivityStatusHistory
{
    public int Id { get; set; }

    [Display(Name = "Activity")]
    public int ActivityId { get; set; }

    public Activity? Activity { get; set; }

    [Required]
    [MaxLength(20)]
    public string OldStatus { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string NewStatus { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(1000)]
    public string? Comment { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace MAAN.SiteOps.Models;

public class ImportBatch
{
    public int Id { get; set; }

    [Display(Name = "Source System")]
    public int SourceSystemId { get; set; }

    public SourceSystem? SourceSystem { get; set; }

    [MaxLength(255)]
    public string? FileName { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = string.Empty;

    public int TotalRecords { get; set; }

    public int LinkedRecords { get; set; }

    public int SkippedRecords { get; set; }

    public int ErrorRecords { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public ICollection<ActivitySourceRecord> ActivitySourceRecords { get; set; }
        = new List<ActivitySourceRecord>();
}
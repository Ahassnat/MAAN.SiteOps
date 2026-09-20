using System.ComponentModel.DataAnnotations;

namespace MAAN.SiteOps.Models;

public class SourceSystem
{
    public int Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ImportBatch> ImportBatches { get; set; }
        = new List<ImportBatch>();

    public ICollection<ActivitySourceRecord> ActivitySourceRecords { get; set; }
        = new List<ActivitySourceRecord>();
}
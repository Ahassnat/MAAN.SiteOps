using System.ComponentModel.DataAnnotations;

namespace MAAN.SiteOps.Models;

public class ActivitySourceRecord
{
    public int Id { get; set; }

    [Display(Name = "Source System")]
    public int SourceSystemId { get; set; }

    public SourceSystem? SourceSystem { get; set; }

    [Display(Name = "Import Batch")]
    public int ImportBatchId { get; set; }

    public ImportBatch? ImportBatch { get; set; }

    [Display(Name = "Activity")]
    public int? ActivityId { get; set; }

    public Activity? Activity { get; set; }

    [MaxLength(255)]
    public string? ExternalRecordId { get; set; }

    [MaxLength(128)]
    public string? RecordHash { get; set; }

    [MaxLength(100)]
    public string? SourceSiteCode { get; set; }

    [MaxLength(200)]
    public string? SourceActivityType { get; set; }

    [MaxLength(100)]
    public string? SourceDonorLabel { get; set; }

    [MaxLength(200)]
    public string? SourceProjectLabel { get; set; }

    [DataType(DataType.Date)]
    public DateTime? SourceDate { get; set; }

    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;

    public string? RawPayloadJson { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace MAAN.SiteOps.Models;

public class Activity
{
    public const string DraftStatus = "Draft";
    public const string SubmittedStatus = "Submitted";
    public const string ReturnedStatus = "Returned";
    public const string ApprovedStatus = "Approved";
    public const string CancelledStatus = "Cancelled";

    public const string ImplementedByMaanRole = "ImplementedByMAAN";
    public const string FacilitatedByMaanRole = "FacilitatedByMAAN";
    public const string CoordinatedByMaanRole = "CoordinatedByMAAN";
    public const string ReferredByMaanRole = "ReferredByMAAN";
    public const string CommunityLedWithMaanSupportRole = "CommunityLedWithMAANSupport";

    public int Id { get; set; }

    [Display(Name = "Project Site")]
    public int ProjectSiteId { get; set; }

    public ProjectSite? ProjectSite { get; set; }

    [Display(Name = "Activity Type")]
    public int ActivityTypeId { get; set; }

    public ActivityType? ActivityType { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Activity Date")]
    public DateTime ActivityDate { get; set; }

    [MaxLength(2000)]
    public string? Summary { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = DraftStatus;

    [Required]
    [MaxLength(50)]
    [Display(Name = "Implementation Role")]
    public string ImplementationRole { get; set; } = ImplementedByMaanRole;

    [ScaffoldColumn(false)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ScaffoldColumn(false)]
    public DateTime? UpdatedAt { get; set; }

    [ScaffoldColumn(false)]
    public DateTime? SubmittedAt { get; set; }

    [ScaffoldColumn(false)]
    public DateTime? ReviewedAt { get; set; }

    [MaxLength(2000)]
    public string? ReviewNotes { get; set; }

    public ActivityParticipant? Participant { get; set; }

    public ICollection<ActivityStatusHistory> StatusHistory { get; set; }
        = new List<ActivityStatusHistory>();

    public ICollection<ActivityProjectAttribution> ProjectAttributions { get; set; }
        = new List<ActivityProjectAttribution>();

    public ICollection<ActivityIndicatorContribution> IndicatorContributions { get; set; }
        = new List<ActivityIndicatorContribution>();

    public ICollection<ActivitySourceRecord> SourceRecords { get; set; }
        = new List<ActivitySourceRecord>();
}
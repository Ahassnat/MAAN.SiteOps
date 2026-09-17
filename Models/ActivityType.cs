using System.ComponentModel.DataAnnotations;

namespace MAAN.SiteOps.Models;

public class ActivityType
{
    public int Id { get; set; }

    [Display(Name = "Activity Category")]
    public int ActivityCategoryId { get; set; }

    public ActivityCategory? ActivityCategory { get; set; }

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public bool RequiresParticipants { get; set; }

    public bool RequiresPWD { get; set; }

    public bool RequiresTopic { get; set; }

    public bool AllowsMultipleTopics { get; set; }

    public bool RequiresItems { get; set; }

    public bool RequiresAttachment { get; set; }

    public bool RequiresSummary { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Activity> Activities { get; set; }
        = new List<Activity>();
}
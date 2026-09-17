using System.ComponentModel.DataAnnotations;

namespace MAAN.SiteOps.Models;

public class ActivityCategory
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ActivityType> ActivityTypes { get; set; }
        = new List<ActivityType>();
}
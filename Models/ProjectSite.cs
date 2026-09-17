using System.ComponentModel.DataAnnotations;

namespace MAAN.SiteOps.Models;

public class ProjectSite
{
    public int Id { get; set; }

    [Display(Name = "Project")]
    public int ProjectId { get; set; }

    public Project? Project { get; set; }

    [Display(Name = "Site")]
    public int SiteId { get; set; }

    public Site? Site { get; set; }

    [Display(Name = "Support Start Date")]
    [DataType(DataType.Date)]
    public DateTime? SupportStartDate { get; set; }

    [Display(Name = "Support End Date")]
    [DataType(DataType.Date)]
    public DateTime? SupportEndDate { get; set; }

    [Display(Name = "Site Cohort")]
    [MaxLength(50)]
    public string SiteCohort { get; set; } = "New";

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Notes { get; set; }
}
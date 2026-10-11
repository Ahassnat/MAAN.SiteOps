using System.ComponentModel.DataAnnotations;

namespace MAAN.SiteOps.ViewModels;

public class ActivityFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Project is required.")]
    [Display(Name = "Project")]
    public int? ProjectId { get; set; }

    [Required(ErrorMessage = "Site is required.")]
    [Display(Name = "Site")]
    public int? ProjectSiteId { get; set; }

    public int? SiteId { get; set; }

    [Required(ErrorMessage = "Activity type is required.")]
    [Display(Name = "Activity Type")]
    public int? ActivityTypeId { get; set; }

    [Required(ErrorMessage = "Activity date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Activity Date")]
    public DateTime? ActivityDate { get; set; }

    [Required(ErrorMessage = "Implementation role is required.")]
    [Display(Name = "Implementation Role")]
    public string ImplementationRole { get; set; } = Models.Activity.ImplementedByMaanRole;

    [MaxLength(2000)]
    public string? Summary { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    [Range(0, int.MaxValue)]
    public int Men { get; set; }

    [Range(0, int.MaxValue)]
    public int Women { get; set; }

    [Range(0, int.MaxValue)]
    public int Boys { get; set; }

    [Range(0, int.MaxValue)]
    public int Girls { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Elderly Men")]
    public int ElderlyMen { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Elderly Women")]
    public int ElderlyWomen { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Persons with Disabilities")]
    public int PersonsWithDisabilities { get; set; }

    public int TotalParticipants => Men + Women + Boys + Girls + ElderlyMen + ElderlyWomen;

    public bool ShowParticipants { get; set; }

    public bool SummaryRequired { get; set; }

    public string? DuplicateWarning { get; set; }
}
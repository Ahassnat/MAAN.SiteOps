using System.ComponentModel.DataAnnotations;

namespace MAAN.SiteOps.Models;

public class ActivityParticipant
{
    public int Id { get; set; }

    [Display(Name = "Activity")]
    public int ActivityId { get; set; }

    public Activity? Activity { get; set; }

    [Range(0, int.MaxValue)]
    public int Men { get; set; }

    [Range(0, int.MaxValue)]
    public int Women { get; set; }

    [Range(0, int.MaxValue)]
    public int Boys { get; set; }

    [Range(0, int.MaxValue)]
    public int Girls { get; set; }

    [Range(0, int.MaxValue)]
    public int ElderlyMen { get; set; }

    [Range(0, int.MaxValue)]
    public int ElderlyWomen { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Persons with Disabilities")]
    public int PersonsWithDisabilities { get; set; }

    public int TotalParticipants => Men + Women + Boys + Girls + ElderlyMen + ElderlyWomen;
}
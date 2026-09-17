using MAAN.SiteOps.Data;
using MAAN.SiteOps.Models;
using MAAN.SiteOps.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MAAN.SiteOps.Controllers;

public class ActivitiesController : Controller
{
    private readonly AppDbContext _context;

    private static readonly IReadOnlyDictionary<string, string> ImplementationRoles =
        new Dictionary<string, string>
        {
            [Activity.ImplementedByMaanRole] = "Implemented by MA'AN",
            [Activity.FacilitatedByMaanRole] = "Facilitated by MA'AN",
            [Activity.CoordinatedByMaanRole] = "Coordinated by MA'AN",
            [Activity.ReferredByMaanRole] = "Referred by MA'AN",
            [Activity.CommunityLedWithMaanSupportRole] = "Community-led with MA'AN support"
        };

    private static readonly string[] Statuses =
    [
        Activity.DraftStatus,
        Activity.SubmittedStatus,
        Activity.ReturnedStatus,
        Activity.ApprovedStatus,
        Activity.CancelledStatus
    ];

    public ActivitiesController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(ActivityIndexViewModel filters)
    {
        var query = ActivitiesQuery();

        if (filters.ProjectId.HasValue)
            query = query.Where(a => a.ProjectSite!.ProjectId == filters.ProjectId.Value);

        if (filters.ProjectSiteId.HasValue)
            query = query.Where(a => a.ProjectSiteId == filters.ProjectSiteId.Value);

        if (filters.ActivityTypeId.HasValue)
            query = query.Where(a => a.ActivityTypeId == filters.ActivityTypeId.Value);

        if (!string.IsNullOrWhiteSpace(filters.Status))
            query = query.Where(a => a.Status == filters.Status);

        if (filters.DateFrom.HasValue)
            query = query.Where(a => a.ActivityDate >= filters.DateFrom.Value.Date);

        if (filters.DateTo.HasValue)
            query = query.Where(a => a.ActivityDate < filters.DateTo.Value.Date.AddDays(1));

        filters.Activities = await query
            .OrderByDescending(a => a.ActivityDate)
            .ThenByDescending(a => a.Id)
            .ToListAsync();

        await LoadFilterOptions(filters.ProjectId, filters.ProjectSiteId);
        return View(filters);
    }

    public async Task<IActionResult> Create()
    {
        var model = new ActivityFormViewModel
        {
            ActivityDate = DateTime.Today
        };

        await LoadFormOptions();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ActivityFormViewModel model)
    {
        var activityType = await _context.ActivityTypes
            .Where(t => t.IsActive && t.Id == model.ActivityTypeId)
            .FirstOrDefaultAsync();

        await ValidateForm(model, activityType);

        if (!ModelState.IsValid)
        {
            await LoadFormOptions(model.ProjectId, model.ProjectSiteId, model.ActivityTypeId);
            return View(model);
        }

        var activity = new Activity
        {
            ProjectSiteId = model.ProjectSiteId!.Value,
            ActivityTypeId = model.ActivityTypeId!.Value,
            ActivityDate = model.ActivityDate!.Value.Date,
            ImplementationRole = model.ImplementationRole,
            Summary = model.Summary,
            Notes = model.Notes,
            Status = Activity.DraftStatus
        };

        if (activityType!.RequiresParticipants)
            activity.Participant = ToParticipant(model);

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        if (model.DuplicateWarning is not null)
            TempData["ActivityWarning"] = model.DuplicateWarning;

        return RedirectToAction(nameof(Details), new { id = activity.Id });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
            return NotFound();

        var activity = await ActivitiesQuery()
            .Include(a => a.Participant)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (activity is null)
            return NotFound();

        if (!CanEdit(activity))
            return BadRequest("Only Draft or Returned activities can be edited.");

        var model = ToFormModel(activity);
        await LoadFormOptions(model.ProjectId, model.ProjectSiteId, model.ActivityTypeId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ActivityFormViewModel model)
    {
        if (id != model.Id)
            return NotFound();

        var activity = await _context.Activities
            .Include(a => a.Participant)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (activity is null)
            return NotFound();

        if (!CanEdit(activity))
            return BadRequest("Only Draft or Returned activities can be edited.");

        var activityType = await _context.ActivityTypes
            .Where(t => t.IsActive && t.Id == model.ActivityTypeId)
            .FirstOrDefaultAsync();

        await ValidateForm(model, activityType, id);

        if (!ModelState.IsValid)
        {
            await LoadFormOptions(model.ProjectId, model.ProjectSiteId, model.ActivityTypeId);
            return View(model);
        }

        activity.ProjectSiteId = model.ProjectSiteId!.Value;
        activity.ActivityTypeId = model.ActivityTypeId!.Value;
        activity.ActivityDate = model.ActivityDate!.Value.Date;
        activity.ImplementationRole = model.ImplementationRole;
        activity.Summary = model.Summary;
        activity.Notes = model.Notes;
        activity.UpdatedAt = DateTime.UtcNow;

        if (activityType!.RequiresParticipants)
        {
            activity.Participant ??= new ActivityParticipant();
            CopyParticipant(model, activity.Participant);
        }
        else if (activity.Participant is not null)
        {
            _context.ActivityParticipants.Remove(activity.Participant);
        }

        await _context.SaveChangesAsync();

        if (model.DuplicateWarning is not null)
            TempData["ActivityWarning"] = model.DuplicateWarning;

        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
            return NotFound();

        var activity = await ActivitiesQuery()
            .Include(a => a.Participant)
            .Include(a => a.StatusHistory)
            .FirstOrDefaultAsync(a => a.Id == id);

        return activity is null ? NotFound() : View(activity);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
            return NotFound();

        var activity = await ActivitiesQuery()
            .Include(a => a.Participant)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (activity is null)
            return NotFound();

        if (activity.Status != Activity.DraftStatus)
            return BadRequest("Only Draft activities can be deleted.");

        return View(activity);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var activity = await _context.Activities.FindAsync(id);

        if (activity is null)
            return NotFound();

        if (activity.Status != Activity.DraftStatus)
            return BadRequest("Only Draft activities can be deleted.");

        _context.Activities.Remove(activity);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Submit(int id) => ChangeStatus(id, Activity.SubmittedStatus);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Approve(int id) => ChangeStatus(id, Activity.ApprovedStatus);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Return(int id, string? comment) => ChangeStatus(id, Activity.ReturnedStatus, comment);

    [HttpGet]
    public async Task<IActionResult> GetSitesByProject(int projectId)
    {
        var sites = await _context.ProjectSites
            .AsNoTracking()
            .Where(ps => ps.ProjectId == projectId && ps.IsActive && ps.Site!.IsActive)
            .OrderBy(ps => ps.Site!.SiteCode)
            .Select(ps => new
            {
                id = ps.Id,
                text = ps.Site!.SiteCode + " - " + ps.Site.SiteName
            })
            .ToListAsync();

        return Json(sites);
    }

    [HttpGet]
    public async Task<IActionResult> GetActivityTypeConfig(int activityTypeId)
    {
        var config = await _context.ActivityTypes
            .AsNoTracking()
            .Where(t => t.Id == activityTypeId && t.IsActive)
            .Select(t => new { requiresParticipants = t.RequiresParticipants, requiresSummary = t.RequiresSummary })
            .FirstOrDefaultAsync();

        return config is null ? NotFound() : Json(config);
    }

    private IQueryable<Activity> ActivitiesQuery() => _context.Activities
        .AsNoTracking()
        .Include(a => a.Participant)
        .Include(a => a.ProjectSite!)
            .ThenInclude(ps => ps.Project)
        .Include(a => a.ProjectSite!)
            .ThenInclude(ps => ps.Site)
        .Include(a => a.ActivityType!)
            .ThenInclude(at => at.ActivityCategory);

    private async Task ValidateForm(ActivityFormViewModel model, ActivityType? activityType, int? activityId = null)
    {
        if (!model.ProjectId.HasValue || !model.ProjectSiteId.HasValue)
        {
            ModelState.AddModelError(nameof(model.ProjectSiteId), "Project and Site are required.");
        }
        else if (!await _context.ProjectSites.AnyAsync(ps =>
            ps.Id == model.ProjectSiteId.Value &&
            ps.ProjectId == model.ProjectId.Value &&
            ps.IsActive &&
            ps.Site!.IsActive))
        {
            ModelState.AddModelError(nameof(model.ProjectSiteId), "Select an active site assigned to the selected project.");
        }

        if (model.ActivityDate.HasValue && model.ActivityDate.Value.Date > DateTime.Today.AddDays(1))
            ModelState.AddModelError(nameof(model.ActivityDate), "Activity date cannot be far in the future.");

        if (activityType is null)
            ModelState.AddModelError(nameof(model.ActivityTypeId), "Select an active activity type.");
        else
        {
            model.ShowParticipants = activityType.RequiresParticipants;
            model.SummaryRequired = activityType.RequiresSummary;

            if (activityType.RequiresSummary && string.IsNullOrWhiteSpace(model.Summary))
                ModelState.AddModelError(nameof(model.Summary), "Summary is required for this activity type.");
        }

        if (model.ProjectSiteId.HasValue && model.ActivityTypeId.HasValue && model.ActivityDate.HasValue)
        {
            var duplicate = await _context.Activities.AnyAsync(a =>
                a.ProjectSiteId == model.ProjectSiteId.Value &&
                a.ActivityTypeId == model.ActivityTypeId.Value &&
                a.ActivityDate == model.ActivityDate.Value.Date &&
                (!activityId.HasValue || a.Id != activityId.Value));

            if (duplicate)
            {
                model.DuplicateWarning = "Another activity with the same project site, type, and date already exists. You can still save this activity.";
            }
        }
    }

    private async Task<IActionResult> ChangeStatus(int id, string newStatus, string? comment = null)
    {
        var activity = await _context.Activities.FindAsync(id);
        if (activity is null)
            return NotFound();

        var validTransition = (activity.Status, newStatus) switch
        {
            (Activity.DraftStatus, Activity.SubmittedStatus) => true,
            (Activity.ReturnedStatus, Activity.SubmittedStatus) => true,
            (Activity.SubmittedStatus, Activity.ApprovedStatus) => true,
            (Activity.SubmittedStatus, Activity.ReturnedStatus) => true,
            _ => false
        };

        if (!validTransition)
            return BadRequest("This status transition is not allowed.");

        var oldStatus = activity.Status;
        activity.Status = newStatus;
        activity.UpdatedAt = DateTime.UtcNow;

        if (newStatus == Activity.SubmittedStatus)
            activity.SubmittedAt = DateTime.UtcNow;

        if (newStatus == Activity.ApprovedStatus || newStatus == Activity.ReturnedStatus)
        {
            activity.ReviewedAt = DateTime.UtcNow;
            activity.ReviewNotes = comment;
        }

        activity.StatusHistory.Add(new ActivityStatusHistory
        {
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedAt = DateTime.UtcNow,
            Comment = comment
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task LoadFormOptions(int? projectId = null, int? projectSiteId = null, int? activityTypeId = null)
    {
        ViewBag.ProjectId = new SelectList(
            await _context.Projects.Where(p => p.IsActive).OrderBy(p => p.ProjectCode).ToListAsync(),
            nameof(Project.Id), nameof(Project.ProjectCode), projectId);

        ViewBag.ProjectSiteId = new SelectList(
            (await ActiveProjectSites(projectId).ToListAsync()).Select(ps => new SelectListItem
            {
                Value = ps.Id.ToString(),
                Text = $"{ps.Site!.SiteCode} - {ps.Site.SiteName}"
            }),
            "Value", "Text", projectSiteId?.ToString());

        ViewBag.ActivityTypeId = new SelectList(
            await _context.ActivityTypes.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync(),
            nameof(ActivityType.Id), nameof(ActivityType.Name), activityTypeId);

        ViewBag.ImplementationRoles = new SelectList(
            ImplementationRoles.Select(r => new { Value = r.Key, Text = r.Value }), "Value", "Text");
    }

    private async Task LoadFilterOptions(int? projectId, int? projectSiteId)
    {
        ViewBag.Projects = new SelectList(
            await _context.Projects.Where(p => p.IsActive).OrderBy(p => p.ProjectCode).ToListAsync(),
            nameof(Project.Id), nameof(Project.ProjectCode), projectId);

        ViewBag.Sites = new SelectList(
            (await ActiveProjectSites(projectId).ToListAsync()).Select(ps => new SelectListItem
            {
                Value = ps.Id.ToString(),
                Text = $"{ps.Site!.SiteCode} - {ps.Site.SiteName}"
            }),
            "Value", "Text", projectSiteId?.ToString());

        ViewBag.ActivityTypes = new SelectList(
            await _context.ActivityTypes.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync(),
            nameof(ActivityType.Id), nameof(ActivityType.Name));
        ViewBag.Statuses = new SelectList(Statuses);
    }

    private IQueryable<ProjectSite> ActiveProjectSites(int? projectId) => _context.ProjectSites
        .AsNoTracking()
        .Where(ps => ps.IsActive && ps.Site!.IsActive && (!projectId.HasValue || ps.ProjectId == projectId.Value))
        .Include(ps => ps.Site)
        .OrderBy(ps => ps.Site!.SiteCode);

    private static bool CanEdit(Activity activity) =>
        activity.Status is Activity.DraftStatus or Activity.ReturnedStatus;

    private static ActivityParticipant ToParticipant(ActivityFormViewModel model) => new()
    {
        Men = model.Men,
        Women = model.Women,
        Boys = model.Boys,
        Girls = model.Girls,
        ElderlyMen = model.ElderlyMen,
        ElderlyWomen = model.ElderlyWomen,
        PersonsWithDisabilities = model.PersonsWithDisabilities
    };

    private static void CopyParticipant(ActivityFormViewModel model, ActivityParticipant participant)
    {
        participant.Men = model.Men;
        participant.Women = model.Women;
        participant.Boys = model.Boys;
        participant.Girls = model.Girls;
        participant.ElderlyMen = model.ElderlyMen;
        participant.ElderlyWomen = model.ElderlyWomen;
        participant.PersonsWithDisabilities = model.PersonsWithDisabilities;
    }

    private static ActivityFormViewModel ToFormModel(Activity activity)
    {
        var participant = activity.Participant;
        return new ActivityFormViewModel
        {
            Id = activity.Id,
            ProjectId = activity.ProjectSite?.ProjectId,
            ProjectSiteId = activity.ProjectSiteId,
            ActivityTypeId = activity.ActivityTypeId,
            ActivityDate = activity.ActivityDate,
            ImplementationRole = activity.ImplementationRole,
            Summary = activity.Summary,
            Notes = activity.Notes,
            Men = participant?.Men ?? 0,
            Women = participant?.Women ?? 0,
            Boys = participant?.Boys ?? 0,
            Girls = participant?.Girls ?? 0,
            ElderlyMen = participant?.ElderlyMen ?? 0,
            ElderlyWomen = participant?.ElderlyWomen ?? 0,
            PersonsWithDisabilities = participant?.PersonsWithDisabilities ?? 0,
            ShowParticipants = activity.ActivityType?.RequiresParticipants == true,
            SummaryRequired = activity.ActivityType?.RequiresSummary == true
        };
    }
}
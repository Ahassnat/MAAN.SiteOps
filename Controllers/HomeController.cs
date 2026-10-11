using System.Diagnostics;
using MAAN.SiteOps.Data;
using Microsoft.AspNetCore.Mvc;
using MAAN.SiteOps.Models;
using MAAN.SiteOps.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MAAN.SiteOps.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var activityCounts = await _context.Activities
            .AsNoTracking()
            .GroupBy(a => a.Status)
            .Select(g => new DashboardStatusCount { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var typeCounts = await _context.Activities
            .AsNoTracking()
            .GroupBy(a => a.ActivityType!.Name)
            .Select(g => new DashboardTypeCount { ActivityType = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.ActivityType)
            .Take(8)
            .ToListAsync();

        var recentActivities = await ActivityQuery()
            .OrderByDescending(a => a.ActivityDate)
            .ThenByDescending(a => a.Id)
            .Take(8)
            .ToListAsync();

        var pendingActivities = await ActivityQuery()
            .Where(a => a.Status == MAAN.SiteOps.Models.Activity.SubmittedStatus)
            .OrderBy(a => a.ActivityDate)
            .ThenBy(a => a.Id)
            .Take(8)
            .ToListAsync();

        return View(new DashboardViewModel
        {
            TotalActivities = await _context.Activities.CountAsync(),
            DraftActivities = await _context.Activities.CountAsync(a => a.Status == MAAN.SiteOps.Models.Activity.DraftStatus),
            PendingReviewActivities = await _context.Activities.CountAsync(a => a.Status == MAAN.SiteOps.Models.Activity.SubmittedStatus),
            ReturnedActivities = await _context.Activities.CountAsync(a => a.Status == MAAN.SiteOps.Models.Activity.ReturnedStatus),
            ApprovedActivities = await _context.Activities.CountAsync(a => a.Status == MAAN.SiteOps.Models.Activity.ApprovedStatus),
            ActiveProjects = await _context.Projects.CountAsync(p => p.IsActive),
            ActiveSites = await _context.Sites.CountAsync(s => s.IsActive),
            StatusCounts = activityCounts,
            TypeCounts = typeCounts,
            RecentActivities = recentActivities,
            PendingActivities = pendingActivities
        });
    }

    private IQueryable<MAAN.SiteOps.Models.Activity> ActivityQuery() => _context.Activities
        .AsNoTracking()
        .Include(a => a.Site)
        .Include(a => a.ProjectSite)
            .ThenInclude(ps => ps.Project)
        .Include(a => a.ActivityType);

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

using MAAN.SiteOps.Data;
using MAAN.SiteOps.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MAAN.SiteOps.Controllers;

public class ProjectSitesController : Controller
{
    private readonly AppDbContext _context;

    public ProjectSitesController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var projectSites = _context.ProjectSites
            .Include(p => p.Project)
            .Include(p => p.Site)
            .ThenInclude(s => s!.Governorate);

        return View(await projectSites.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var projectSite = await _context.ProjectSites
            .Include(p => p.Project)
            .Include(p => p.Site)
            .ThenInclude(s => s!.Governorate)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (projectSite == null)
            return NotFound();

        return View(projectSite);
    }

    public IActionResult Create()
{
    LoadDropdowns();

    var model = new ProjectSite
    {
        IsActive = true,
        SupportStartDate = DateTime.Today
    };

    return View(model);
}

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectSite projectSite)
    {
        var alreadyExists = await _context.ProjectSites.AnyAsync(x =>
            x.ProjectId == projectSite.ProjectId &&
            x.SiteId == projectSite.SiteId);

        if (alreadyExists)
        {
            ModelState.AddModelError(
                "",
                "This site is already assigned to the selected project."
            );
        }

        if (ModelState.IsValid)
        {
            _context.Add(projectSite);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        LoadDropdowns(projectSite.ProjectId, projectSite.SiteId);

        return View(projectSite);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var projectSite = await _context.ProjectSites.FindAsync(id);

        if (projectSite == null)
            return NotFound();

        LoadDropdowns(projectSite.ProjectId, projectSite.SiteId);

        return View(projectSite);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProjectSite projectSite)
    {
        if (id != projectSite.Id)
            return NotFound();

        var duplicate = await _context.ProjectSites.AnyAsync(x =>
            x.ProjectId == projectSite.ProjectId &&
            x.SiteId == projectSite.SiteId &&
            x.Id != projectSite.Id);

        if (duplicate)
        {
            ModelState.AddModelError(
                "",
                "This site is already assigned to the selected project."
            );
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(projectSite);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectSiteExists(projectSite.Id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        LoadDropdowns(projectSite.ProjectId, projectSite.SiteId);

        return View(projectSite);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var projectSite = await _context.ProjectSites
            .Include(p => p.Project)
            .Include(p => p.Site)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (projectSite == null)
            return NotFound();

        return View(projectSite);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var projectSite = await _context.ProjectSites.FindAsync(id);

        if (projectSite != null)
            _context.ProjectSites.Remove(projectSite);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private void LoadDropdowns(
        int? projectId = null,
        int? siteId = null)
    {
        var projects = _context.Projects
            .Where(p => p.IsActive)
            .Select(p => new
            {
                p.Id,
                DisplayName = p.ProjectCode + " - " + p.ProjectName
            })
            .ToList();

        var sites = _context.Sites
            .Where(s => s.IsActive)
            .Select(s => new
            {
                s.Id,
                DisplayName = s.SiteCode + " - " + s.SiteName
            })
            .ToList();

        ViewData["ProjectId"] =
            new SelectList(projects, "Id", "DisplayName", projectId);

        ViewData["SiteId"] =
            new SelectList(sites, "Id", "DisplayName", siteId);
    }

    private bool ProjectSiteExists(int id)
    {
        return _context.ProjectSites.Any(e => e.Id == id);
    }
}
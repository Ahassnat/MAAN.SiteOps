using MAAN.SiteOps.Data;
using MAAN.SiteOps.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MAAN.SiteOps.Controllers;

public class ProjectIndicatorsController : Controller
{
    private readonly AppDbContext _context;
    public ProjectIndicatorsController(AppDbContext context) => _context = context;

    public async Task<IActionResult> Index(int projectId)
    {
        ViewBag.Project = await _context.Projects.FindAsync(projectId);
        return View(await _context.ProjectIndicators.Include(x => x.Indicator).Where(x => x.ProjectId == projectId).OrderBy(x => x.ProjectIndicatorCode).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var item = await _context.ProjectIndicators.Include(x => x.Project).Include(x => x.Indicator).FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound() : View(item);
    }

    public async Task<IActionResult> Create(int projectId)
    {
        ViewBag.ProjectId = projectId;
        await LoadIndicators();
        return View(new ProjectIndicator { ProjectId = projectId, IsActive = true });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectIndicator model)
    {
        if (ModelState.IsValid) { _context.ProjectIndicators.Add(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index), new { projectId = model.ProjectId }); }
        ViewBag.ProjectId = model.ProjectId; await LoadIndicators(model.IndicatorId); return View(model);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var item = await _context.ProjectIndicators.FindAsync(id);
        if (item is null) return NotFound();
        await LoadIndicators(item.IndicatorId); return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProjectIndicator model)
    {
        if (id != model.Id) return NotFound();
        if (ModelState.IsValid) { _context.Update(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index), new { projectId = model.ProjectId }); }
        await LoadIndicators(model.IndicatorId); return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var item = await _context.ProjectIndicators.FindAsync(id);
        if (item is null) return NotFound(); item.IsActive = false; await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { projectId = item.ProjectId });
    }

    private async Task LoadIndicators(int? selected = null) => ViewBag.IndicatorId = new SelectList(await _context.Indicators.Where(i => i.IsActive).OrderBy(i => i.Name).ToListAsync(), "Id", "Name", selected);
}
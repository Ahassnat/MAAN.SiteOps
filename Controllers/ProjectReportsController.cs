using MAAN.SiteOps.Data;
using MAAN.SiteOps.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MAAN.SiteOps.Controllers;

public class ProjectReportsController : Controller
{
    private readonly AppDbContext _context;
    public ProjectReportsController(AppDbContext context) => _context = context;

    public async Task<IActionResult> Index(int projectId) { ViewBag.Project = await _context.Projects.FindAsync(projectId); return View(await _context.ProjectReports.Include(x => x.ReportDefinition).Where(x => x.ProjectId == projectId).OrderBy(x => x.ReportDefinition!.Name).ToListAsync()); }
    public async Task<IActionResult> Create(int projectId) { ViewBag.ProjectId = projectId; ViewBag.ReportDefinitionId = new SelectList(await _context.ReportDefinitions.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(), "Id", "Name"); return View(new ProjectReport { ProjectId = projectId, IsActive = true }); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectReport model) { if (ModelState.IsValid) { _context.Add(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index), new { projectId = model.ProjectId }); } ViewBag.ProjectId = model.ProjectId; ViewBag.ReportDefinitionId = new SelectList(await _context.ReportDefinitions.Where(x => x.IsActive).ToListAsync(), "Id", "Name"); return View(model); }
    public async Task<IActionResult> Edit(int? id) { if (id is null) return NotFound(); var item = await _context.ProjectReports.FindAsync(id); if (item is null) return NotFound(); ViewBag.ReportDefinitionId = new SelectList(await _context.ReportDefinitions.Where(x => x.IsActive).ToListAsync(), "Id", "Name", item.ReportDefinitionId); return View(item); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProjectReport model) { if (id != model.Id) return NotFound(); if (ModelState.IsValid) { _context.Update(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index), new { projectId = model.ProjectId }); } ViewBag.ReportDefinitionId = new SelectList(await _context.ReportDefinitions.Where(x => x.IsActive).ToListAsync(), "Id", "Name", model.ReportDefinitionId); return View(model); }
    public async Task<IActionResult> Mappings(int id) { var report = await _context.ProjectReports.Include(x => x.ReportDefinition).Include(x => x.Project).FirstOrDefaultAsync(x => x.Id == id); if (report is null) return NotFound(); ViewBag.Indicators = await _context.ProjectIndicators.Include(x => x.Indicator).Where(x => x.ProjectId == report.ProjectId && x.IsActive).OrderBy(x => x.ProjectIndicatorCode).ToListAsync(); return View(report); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMapping(int id, int projectIndicatorId, string reportFieldCode, string reportFieldName, int displayOrder, string? notes)
    {
        if (!await _context.ProjectReports.AnyAsync(x => x.Id == id)) return NotFound();
        if (string.IsNullOrWhiteSpace(reportFieldCode))
        {
            TempData["MappingError"] = "Report field code is required.";
            return RedirectToAction(nameof(Mappings), new { id });
        }
        if (await _context.ReportIndicatorMappings.AnyAsync(x => x.ProjectReportId == id && x.ReportFieldCode == reportFieldCode))
        {
            TempData["MappingError"] = "That report field code is already mapped in this report.";
            return RedirectToAction(nameof(Mappings), new { id });
        }
        _context.Add(new ReportIndicatorMapping { ProjectReportId = id, ProjectIndicatorId = projectIndicatorId, ReportFieldCode = reportFieldCode, ReportFieldName = reportFieldName, DisplayOrder = displayOrder, Notes = notes });
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Mappings), new { id });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMapping(int id) { var mapping = await _context.ReportIndicatorMappings.FindAsync(id); if (mapping is null) return NotFound(); var reportId = mapping.ProjectReportId; _context.Remove(mapping); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Mappings), new { id = reportId }); }
}
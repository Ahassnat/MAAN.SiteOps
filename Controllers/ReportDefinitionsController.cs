using MAAN.SiteOps.Data;
using MAAN.SiteOps.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MAAN.SiteOps.Controllers;

public class ReportDefinitionsController : Controller
{
    private readonly AppDbContext _context;
    public ReportDefinitionsController(AppDbContext context) => _context = context;
    public async Task<IActionResult> Index() => View(await _context.ReportDefinitions.OrderBy(x => x.Code).ToListAsync());
    public async Task<IActionResult> Details(int? id) => id is null ? NotFound() : View(await _context.ReportDefinitions.FirstOrDefaultAsync(x => x.Id == id));
    public IActionResult Create() => View(new ReportDefinition());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReportDefinition model) { if (ModelState.IsValid) { _context.Add(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); } return View(model); }
    public async Task<IActionResult> Edit(int? id) => id is null ? NotFound() : View(await _context.ReportDefinitions.FindAsync(id));
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ReportDefinition model) { if (id != model.Id) return NotFound(); if (ModelState.IsValid) { model.UpdatedAt = DateTime.UtcNow; _context.Update(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); } return View(model); }
}
using MAAN.SiteOps.Data;
using MAAN.SiteOps.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MAAN.SiteOps.Controllers;

public class ActivityTypesController : Controller
{
    private readonly AppDbContext _context;
    public ActivityTypesController(AppDbContext context) => _context = context;
    public async Task<IActionResult> Index() => View(await _context.ActivityTypes.Include(x => x.ActivityCategory).OrderBy(x => x.Name).ToListAsync());
    public async Task<IActionResult> Create() { await LoadCategories(); return View(new ActivityType { IsActive = true }); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ActivityType model) { if (ModelState.IsValid) { _context.Add(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); } await LoadCategories(model.ActivityCategoryId); return View(model); }
    public async Task<IActionResult> Edit(int? id) { if (id is null) return NotFound(); var item = await _context.ActivityTypes.FindAsync(id); if (item is null) return NotFound(); await LoadCategories(item.ActivityCategoryId); return View(item); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ActivityType model) { if (id != model.Id) return NotFound(); if (ModelState.IsValid) { _context.Update(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); } await LoadCategories(model.ActivityCategoryId); return View(model); }
    private async Task LoadCategories(int? selected = null) => ViewBag.ActivityCategoryId = new SelectList(await _context.ActivityCategories.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToListAsync(), "Id", "Name", selected);
}
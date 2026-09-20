using MAAN.SiteOps.Data;
using MAAN.SiteOps.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MAAN.SiteOps.Controllers;

public class ActivityCategoriesController : Controller
{
    private readonly AppDbContext _context;
    public ActivityCategoriesController(AppDbContext context) => _context = context;
    public async Task<IActionResult> Index() => View(await _context.ActivityCategories.Include(x => x.ActivityTypes).OrderBy(x => x.DisplayOrder).ToListAsync());
    public IActionResult Create() => View(new ActivityCategory());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ActivityCategory model) { if (ModelState.IsValid) { _context.Add(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); } return View(model); }
    public async Task<IActionResult> Edit(int? id) => id is null ? NotFound() : View(await _context.ActivityCategories.FindAsync(id));
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ActivityCategory model) { if (id != model.Id) return NotFound(); if (ModelState.IsValid) { _context.Update(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); } return View(model); }
}
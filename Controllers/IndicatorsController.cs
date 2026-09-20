using MAAN.SiteOps.Data;
using MAAN.SiteOps.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MAAN.SiteOps.Controllers;

public class IndicatorsController : Controller
{
    private readonly AppDbContext _context;
    public IndicatorsController(AppDbContext context) => _context = context;

    public async Task<IActionResult> Index() => View(await _context.Indicators.OrderBy(i => i.Name).ToListAsync());
    public async Task<IActionResult> Details(int? id) => id is null ? NotFound() : View(await _context.Indicators.FirstOrDefaultAsync(i => i.Id == id));
    public IActionResult Create() => View(new Indicator());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Indicator indicator)
    {
        if (ModelState.IsValid) { _context.Indicators.Add(indicator); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
        return View(indicator);
    }

    public async Task<IActionResult> Edit(int? id) => id is null ? NotFound() : View(await _context.Indicators.FindAsync(id));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Indicator indicator)
    {
        if (id != indicator.Id) return NotFound();
        if (ModelState.IsValid) { indicator.UpdatedAt = DateTime.UtcNow; _context.Update(indicator); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index)); }
        return View(indicator);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var indicator = await _context.Indicators.FindAsync(id);
        if (indicator is null) return NotFound();
        indicator.IsActive = false; indicator.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index));
    }
}
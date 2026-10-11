using MAAN.SiteOps.Data;
using MAAN.SiteOps.Services;
using MAAN.SiteOps.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MAAN.SiteOps.Controllers;

public class ImportsController : Controller
{
    private readonly AppDbContext _context;
    private readonly IDataImportService _importService;

    public ImportsController(AppDbContext context, IDataImportService importService)
    {
        _context = context;
        _importService = importService;
    }

    public async Task<IActionResult> Index()
    {
        var batches = await _context.ImportBatches
            .Include(x => x.SourceSystem)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync();
        ViewBag.SourceSystems = new SelectList(await _context.SourceSystems.Where(x => x.IsActive && x.Code == "ZITE").ToListAsync(), "Code", "Name");
        return View(batches);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Preview(string sourceSystemCode, IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            TempData["ImportError"] = "Select an Excel file to preview.";
            return RedirectToAction(nameof(Index));
        }

        if (!string.Equals(sourceSystemCode, "ZITE", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ImportError"] = "Only the ZITE Activity Report preview is supported in this phase.";
            return RedirectToAction(nameof(Index));
        }

        await using var stream = file.OpenReadStream();
        var preview = await _importService.PreviewAsync(stream, sourceSystemCode, file.FileName, cancellationToken);
        var source = await _context.SourceSystems.SingleAsync(x => x.Code == sourceSystemCode, cancellationToken);
        var batch = new Models.ImportBatch
        {
            SourceSystemId = source.Id,
            FileName = file.FileName,
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            Status = "Preview",
            TotalRecords = preview.RecordsRead,
            LinkedRecords = preview.ReadyForImport,
            SkippedRecords = preview.UnresolvedSites + preview.UnresolvedActivityTypes,
            ErrorRecords = preview.RecordsWithWarnings
        };
        _context.ImportBatches.Add(batch);
        await _context.SaveChangesAsync(cancellationToken);
        preview.BatchId = batch.Id;
        return View(preview);
    }

    public async Task<IActionResult> Details(int id)
    {
        var batch = await _context.ImportBatches.Include(x => x.SourceSystem).FirstOrDefaultAsync(x => x.Id == id);
        return batch is null ? NotFound() : View(batch);
    }
}
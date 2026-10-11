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
    private readonly SiteListPreviewStore _sitePreviewStore;

    public ImportsController(AppDbContext context, IDataImportService importService, SiteListPreviewStore sitePreviewStore)
    {
        _context = context;
        _importService = importService;
        _sitePreviewStore = sitePreviewStore;
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

    public async Task<IActionResult> ResolveHistoricalSite(string? siteCode, string? siteName, int activityRecordCount, DateTime? earliestActivityDate, DateTime? latestActivityDate)
    {
        var existing = await _context.Sites.FirstOrDefaultAsync(x => x.SiteCode == siteCode);
        if (existing is not null)
        {
            TempData["ImportMessage"] = $"SiteCode {existing.SiteCode} already exists. No Site was created.";
            return RedirectToAction(nameof(Index));
        }

        await LoadGovernorates();
        return View(new HistoricalSiteResolutionViewModel
        {
            SiteCode = siteCode ?? string.Empty,
            SiteName = siteName ?? string.Empty,
            ActivityRecordCount = activityRecordCount,
            EarliestActivityDate = earliestActivityDate,
            LatestActivityDate = latestActivityDate
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateHistoricalSite(HistoricalSiteResolutionViewModel model)
    {
        if (await _context.Sites.AnyAsync(x => x.SiteCode == model.SiteCode))
        {
            TempData["ImportMessage"] = $"SiteCode {model.SiteCode} already exists. No duplicate Site was created.";
            return RedirectToAction(nameof(Index));
        }

        if (model.GovernorateId.HasValue && !await _context.Governorates.AnyAsync(x => x.Id == model.GovernorateId.Value))
            ModelState.AddModelError(nameof(model.GovernorateId), "Select an existing Governorate.");

        if (!ModelState.IsValid)
        {
            await LoadGovernorates(model.GovernorateId);
            return View("ResolveHistoricalSite", model);
        }

        _context.Sites.Add(new Models.Site
        {
            SiteCode = model.SiteCode.Trim(),
            SiteName = model.SiteName.Trim(),
            GovernorateId = model.GovernorateId!.Value,
            Area = string.IsNullOrWhiteSpace(model.Area) ? null : model.Area.Trim(),
            Latitude = model.Latitude,
            Longitude = model.Longitude,
            IsActive = false,
            IsManagedByMaan = true
        });
        await _context.SaveChangesAsync();
        TempData["ImportMessage"] = $"Historical Site {model.SiteCode} was created inactive. No project assignments or Activities were created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PreviewSiteList(string sourceSystemCode, IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0 || !string.Equals(sourceSystemCode, "ZITE", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ImportError"] = file is null || file.Length == 0 ? "Select an Excel file to preview." : "Only ZITE Site List preview is supported.";
            return RedirectToAction(nameof(Index));
        }

        await using var stream = file.OpenReadStream();
        var preview = await _importService.PreviewSitesAsync(stream, sourceSystemCode, file.FileName, cancellationToken);
        var source = await _context.SourceSystems.SingleAsync(x => x.Code == sourceSystemCode, cancellationToken);
        var batch = new Models.ImportBatch
        {
            SourceSystemId = source.Id,
            FileName = file.FileName,
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            Status = "Preview",
            TotalRecords = preview.SitesRead,
            LinkedRecords = preview.ExistingSites,
            SkippedRecords = preview.Errors + preview.UnresolvedGovernorates,
            ErrorRecords = preview.Errors + preview.UnresolvedGovernorates
        };
        _context.ImportBatches.Add(batch);
        await _context.SaveChangesAsync(cancellationToken);
        preview.BatchId = batch.Id;
        _sitePreviewStore.Save(preview);
        return View("SiteListPreview", preview);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportSiteList(Guid previewToken, CancellationToken cancellationToken)
    {
        if (!_sitePreviewStore.Remove(previewToken, out var preview) || preview is null)
        {
            TempData["ImportError"] = "This Site List preview has expired. Upload the workbook and preview it again.";
            return RedirectToAction(nameof(Index));
        }

        var imported = await _importService.ImportSitesAsync(preview, cancellationToken);
        var batch = await _context.ImportBatches.FindAsync([preview.BatchId], cancellationToken);
        if (batch is not null)
        {
            batch.Status = "Completed";
            batch.CompletedAt = DateTime.UtcNow;
            batch.LinkedRecords = imported;
            batch.SkippedRecords = preview.Errors + preview.UnresolvedGovernorates;
            batch.ErrorRecords = preview.Errors;
            batch.Notes = $"ZITE Site List: {imported} new sites added; {preview.SitesWithChanges} existing sites updated.";
            await _context.SaveChangesAsync(cancellationToken);
        }

        TempData["ImportMessage"] = $"Site List import complete. {imported} sites were added or updated. No Project Site assignments were created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var batch = await _context.ImportBatches.Include(x => x.SourceSystem).FirstOrDefaultAsync(x => x.Id == id);
        return batch is null ? NotFound() : View(batch);
    }

    private async Task LoadGovernorates(int? selected = null)
    {
        ViewBag.GovernorateId = new SelectList(await _context.Governorates.OrderBy(x => x.Name).ToListAsync(), "Id", "Name", selected);
    }
}
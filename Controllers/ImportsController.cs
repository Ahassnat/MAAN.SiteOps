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
            SkippedRecords = preview.UnresolvedSites + preview.UnresolvedActivityTypes + preview.AlreadyImported,
            ErrorRecords = preview.RecordsWithWarnings
        };
        _context.ImportBatches.Add(batch);
        await _context.SaveChangesAsync(cancellationToken);
        preview.BatchId = batch.Id;
        preview.SourceImportToken = _sitePreviewStore.SaveActivityPreview(new ActivitySourceImportPreview(
            batch.Id,
            source.Id,
            file.FileName,
            preview.Records.Select(row => new ActivitySourcePreviewRow(
                row.RowNumber,
                row.ExternalRecordId,
                row.RecordHash,
                row.SourceSiteCode,
                row.SourceSiteName,
                row.MatchedSiteId,
                row.SourceActivityType,
                row.MatchedActivityTypeId,
                row.ActivityDate,
                row.SourceDonorLabel,
                row.Men,
                row.Women,
                row.Boys,
                row.Girls,
                row.ElderlyMen,
                row.ElderlyWomen,
                row.PersonsWithDisabilities,
                row.SourceTotalParticipants,
                row.ParticipantsTotal,
                row.Summary,
                row.Notes,
                row.Agency,
                row.SourceActivityCategory,
                row.SourceTopic,
                row.Warnings.ToList(),
                row.DuplicateStatus,
                row.ReadyForImport)).ToList()));
        preview.AlreadyImported = preview.Records.Count(row => row.DuplicateStatus == "AlreadyImported");
        preview.ReadyForImport = preview.Records.Count(row => row.ReadyForImport && row.DuplicateStatus != "AlreadyImported");
        batch.LinkedRecords = preview.ReadyForImport;
        batch.SkippedRecords = preview.UnresolvedSites + preview.UnresolvedActivityTypes + preview.AlreadyImported;
        await _context.SaveChangesAsync(cancellationToken);
        return View(preview);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportActivitySourceRecords(Guid previewToken, CancellationToken cancellationToken)
    {
        if (!_sitePreviewStore.RemoveActivityPreview(previewToken, out var snapshot) || snapshot is null)
        {
            TempData["ImportError"] = "This Activity Report preview expired. Upload and preview the workbook again.";
            return RedirectToAction(nameof(Index));
        }

        var batch = await _context.ImportBatches.FindAsync([snapshot.BatchId], cancellationToken);
        if (batch is null) return NotFound();

        var imported = 0;
        var alreadyImported = 0;
        var ready = 0;
        var needsReview = 0;
        var errors = 0;
        var exactSourceDuplicates = snapshot.Rows.Count(row => row.DuplicateStatus == "ExactSourceDuplicate");
        var seenExternalIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenRecordHashes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in snapshot.Rows)
        {
            var duplicateExists = !string.IsNullOrWhiteSpace(row.ExternalRecordId)
            ? !seenExternalIds.Add(row.ExternalRecordId) || await _context.ActivitySourceRecords.AnyAsync(x => x.SourceSystemId == snapshot.SourceSystemId && x.ExternalRecordId == row.ExternalRecordId, cancellationToken)
            : !string.IsNullOrWhiteSpace(row.RecordHash) && (!seenRecordHashes.Add(row.RecordHash) || await _context.ActivitySourceRecords.AnyAsync(x => x.SourceSystemId == snapshot.SourceSystemId && x.ExternalRecordId == null && x.RecordHash == row.RecordHash, cancellationToken));

            if (duplicateExists)
            {
                alreadyImported++;
                continue;
            }

            var status = row.IsReady && row.Warnings.Count == 0 && row.DuplicateStatus == "NoDuplicateDetected"
                ? "ReadyForNormalization"
                : "NeedsReview";
            if (status == "ReadyForNormalization") ready++; else needsReview++;

            var rawPayload = System.Text.Json.JsonSerializer.Serialize(new
            {
                row.ExternalRecordId,
                row.SourceSiteCode,
                row.SourceSiteName,
                row.SourceActivityType,
                row.ActivityCategory,
                row.Agency,
                SourceDonorLabel = row.SourceDonorLabel,
                row.SourceDate,
                row.Men,
                row.Women,
                row.Boys,
                row.Girls,
                row.ElderlyMen,
                row.ElderlyWomen,
                row.PersonsWithDisabilities,
                row.SourceTotalParticipants,
                row.ParticipantsTotal,
                row.Summary,
                row.Notes,
                row.Warnings,
                row.DuplicateStatus
            });

            _context.ActivitySourceRecords.Add(new Models.ActivitySourceRecord
            {
                SourceSystemId = snapshot.SourceSystemId,
                ImportBatchId = snapshot.BatchId,
                ActivityId = null,
                ExternalRecordId = row.ExternalRecordId,
                RecordHash = row.RecordHash,
                SourceSiteCode = row.SourceSiteCode,
                SourceActivityType = row.SourceActivityType,
                SourceDonorLabel = row.SourceDonorLabel,
                SourceDate = row.SourceDate,
                ImportedAt = DateTime.UtcNow,
                RawPayloadJson = rawPayload,
                Notes = row.Warnings.Count == 0 ? null : string.Join("; ", row.Warnings),
                ProcessingStatus = status,
                WarningsJson = row.Warnings.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(row.Warnings)
            });
            imported++;
        }

        batch.Status = "Completed";
        batch.CompletedAt = DateTime.UtcNow;
        batch.TotalRecords = snapshot.Rows.Count;
        batch.LinkedRecords = imported;
        batch.SkippedRecords = alreadyImported;
        batch.ErrorRecords = errors;
        batch.Notes = $"ZITE Activity Report source ingestion. Imported {imported}; ready for normalization {ready}; needs review {needsReview}; already imported/skipped {alreadyImported}; errors {errors}; Activities created: 0.";
        await _context.SaveChangesAsync(cancellationToken);

        TempData["SourceImportResult"] = System.Text.Json.JsonSerializer.Serialize(new SourceRecordImportResultViewModel
        {
            BatchId = batch.Id,
            SourceRecordsRead = snapshot.Rows.Count,
            SourceRecordsImported = imported,
            AlreadyImportedOrSkipped = alreadyImported,
            ExactSourceDuplicates = exactSourceDuplicates,
            ReadyForNormalization = ready,
            NeedsReview = needsReview,
            Errors = errors,
            ActivitiesCreated = 0
        });
        return RedirectToAction(nameof(SourceImportResult));
    }

    public IActionResult SourceImportResult()
    {
        var json = TempData["SourceImportResult"] as string;
        if (string.IsNullOrWhiteSpace(json)) return RedirectToAction(nameof(Index));
        return View(System.Text.Json.JsonSerializer.Deserialize<SourceRecordImportResultViewModel>(json)!);
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
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ClosedXML.Excel;
using MAAN.SiteOps.Data;
using MAAN.SiteOps.Models;
using MAAN.SiteOps.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MAAN.SiteOps.Services;

public sealed class ZiteExcelImportService : IDataImportService
{
    private readonly AppDbContext _context;
    public ZiteExcelImportService(AppDbContext context) => _context = context;

    public async Task<ImportPreviewViewModel> PreviewAsync(Stream workbook, string sourceSystemCode, string fileName, CancellationToken cancellationToken = default)
    {
        var result = new ImportPreviewViewModel { SourceSystemCode = sourceSystemCode, FileName = fileName };
        try
        {
            using var package = new XLWorkbook(workbook);
            var sheet = package.Worksheets.FirstOrDefault(x => Normalize(x.Name) == "activityreport");
            if (sheet is null) { result.Error = "The workbook does not contain an Activity Report sheet."; return result; }
            result.DetectedSheet = sheet.Name;
            var used = sheet.RangeUsed();
            if (used is null) { result.Error = "The Activity Report sheet is empty."; return result; }
            var headers = HeaderMap(used.FirstRow());
            var sites = await _context.Sites.AsNoTracking().ToListAsync(cancellationToken);
            var types = await _context.ActivityTypes.AsNoTracking().Where(x => x.IsActive).ToListAsync(cancellationToken);
            var source = await _context.SourceSystems.AsNoTracking().FirstOrDefaultAsync(x => x.Code == sourceSystemCode, cancellationToken);
            var importedIds = source is null ? [] : (await _context.ActivitySourceRecords.AsNoTracking().Where(x => x.SourceSystemId == source.Id && x.ExternalRecordId != null).Select(x => x.ExternalRecordId!).ToListAsync(cancellationToken)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var records = used.RowsUsed().Skip(1).Select((row, index) => ParseRow(row, headers, sites, types, importedIds, index + 2)).ToList();
            MarkDuplicateGroups(records);
            result.Records = records;
            result.RecordsRead = records.Count;
            result.ReadyForImport = records.Count(x => x.ReadyForImport);
            result.UnresolvedSites = records.Count(x => x.SiteMatchStatus == "Unresolved");
            result.UnresolvedActivityTypes = records.Count(x => x.ActivityTypeMatchStatus == "Unresolved");
            result.ActivityTypesMatched = records.Count(x => x.MatchedActivityTypeId.HasValue);
            result.ActivityTypesUnresolved = records.Count(x => !x.MatchedActivityTypeId.HasValue);
            result.UnresolvedSourceActivityTypes = records.Where(x => !x.MatchedActivityTypeId.HasValue && !string.IsNullOrWhiteSpace(x.SourceActivityType)).Select(x => x.SourceActivityType!).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
            result.PossibleDuplicates = records.Count(x => x.DuplicateStatus == "PossibleDuplicate");
            result.ExactSourceDuplicates = records.Count(x => x.DuplicateStatus == "ExactSourceDuplicate");
            result.AlreadyImported = records.Count(x => x.DuplicateStatus == "AlreadyImported");
            result.RecordsWithWarnings = records.Count(x => x.Warnings.Count > 0);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or InvalidDataException)
        {
            result.Error = "The uploaded file could not be read as a valid Excel workbook.";
        }
        return result;
    }

    private static ImportPreviewRecordViewModel ParseRow(IXLRangeRow row, Dictionary<string, int> headers, List<Site> sites, List<ActivityType> types, HashSet<string> importedIds, int rowNumber)
    {
        string Value(params string[] names) => names.Select(Normalize).Where(headers.ContainsKey).Select(name => row.Cell(headers[name]).GetString().Trim()).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? string.Empty;
        var sourceCode = Value("Site ID", "SiteCode");
        var sourceName = Value("Site Name", "Site");
        var sourceType = Value("Sub-Activity", "Activity Type", "Activity");
        var externalId = Value("Case ID", "External Record ID", "Record ID", "ID");
        var sourceCategory = Value("Primary Activity", "Activity Category", "Category");
        var agency = Value("Agency");
        var donor = Value("Donor", "Source Donor Label");
        var summary = Value("Summary", "Description");
        var notes = Value("Comments", "Notes");
        var dateText = Value("Activity date (reformatted)", "Activity date", "Date");
        DateTime? date = DateTime.TryParse(dateText, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed) ? parsed.Date : null;
        var men = ParseNullableInt(Value("men"));
        var women = ParseNullableInt(Value("women"));
        var boys = ParseNullableInt(Value("boys"));
        var girls = ParseNullableInt(Value("girls"));
        var elderlyMen = ParseNullableInt(Value("elderly men"));
        var elderlyWomen = ParseNullableInt(Value("elderly women"));
        var pwd = ParseNullableInt(Value("PWds", "Persons With Disabilities"));
        var sourceTotal = ParseNullableInt(Value("total participants", "Total Participants"));
        var calculatedTotal = (men ?? 0) + (women ?? 0) + (boys ?? 0) + (girls ?? 0) + (elderlyMen ?? 0) + (elderlyWomen ?? 0);
        var item = new ImportPreviewRecordViewModel { RowNumber = rowNumber, ExternalRecordId = externalId, SourceSiteCode = sourceCode, SourceSiteName = sourceName, SourceActivityType = sourceType, SourceActivityCategory = sourceCategory, Agency = agency, SourceDonorLabel = donor, ActivityDate = date, SourceTotalParticipants = sourceTotal, ParticipantsTotal = calculatedTotal, Summary = summary, Notes = notes, Men = men, Women = women, Boys = boys, Girls = girls, ElderlyMen = elderlyMen, ElderlyWomen = elderlyWomen, PersonsWithDisabilities = pwd, RecordHash = Hash(string.Join("|", "ZITE", externalId, sourceCode, date?.ToString("yyyy-MM-dd"), sourceType, summary)) };
        if (sourceTotal.HasValue && sourceTotal.Value != calculatedTotal) item.Warnings.Add($"Source total participants ({sourceTotal}) differs from calculated total ({calculatedTotal}).");

        var site = sites.FirstOrDefault(x => !string.IsNullOrWhiteSpace(sourceCode) && Normalize(x.SiteCode) == Normalize(sourceCode));
        if (site is not null) { item.MatchedSiteId = site.Id; item.MatchedSiteDisplay = $"{site.SiteCode} - {site.SiteName}"; item.SiteMatchStatus = "Matched"; if (!string.IsNullOrWhiteSpace(sourceName) && Normalize(site.SiteName) != Normalize(sourceName)) { item.SiteMatchStatus = "NameMismatch"; item.Warnings.Add("Site name differs from the matched SiteCode."); } }
        else if (!string.IsNullOrWhiteSpace(sourceName)) { site = sites.FirstOrDefault(x => Normalize(x.SiteName) == Normalize(sourceName)); if (site is not null) { item.MatchedSiteId = site.Id; item.MatchedSiteDisplay = $"{site.SiteCode} - {site.SiteName}"; item.SiteMatchStatus = "NameMismatch"; item.Warnings.Add("Matched by exact site name because SiteCode was unavailable."); } }

        var alias = Normalize(sourceType) switch { "communityledprojectplans" => "communityledactivityplan", "communityledprojects" => "communityledactivity", "siteimprovementplans" => "siteimprovementplan", "siteimprovementscareandmaintenance" => "siteimprovementscaremaintenance", "safetyaudits" => "safetyaudit", "awarenesssessions" => "awarenesssession", _ => Normalize(sourceType) };
        var activityType = types.FirstOrDefault(x => Normalize(x.Name) == alias || Normalize(x.Code) == alias);
        if (activityType is not null) { item.MatchedActivityTypeId = activityType.Id; item.MatchedActivityTypeName = activityType.Name; item.ActivityTypeMatchStatus = alias == Normalize(sourceType) ? "Matched" : "AliasMatched"; }
        if (string.IsNullOrWhiteSpace(sourceType)) item.Warnings.Add("Activity Type is missing.");
        if (!date.HasValue) item.Warnings.Add("Activity Date is missing or invalid.");
        if (!string.IsNullOrWhiteSpace(externalId) && importedIds.Contains(externalId)) item.DuplicateStatus = "AlreadyImported";
        return item;
    }

    private static void MarkDuplicateGroups(List<ImportPreviewRecordViewModel> records)
    {
        foreach (var group in records.Where(x => !string.IsNullOrWhiteSpace(x.SourceSiteCode) && x.MatchedActivityTypeId.HasValue && x.ActivityDate.HasValue).GroupBy(x => $"{Normalize(x.SourceSiteCode!)}|{x.MatchedActivityTypeId}|{x.ActivityDate:yyyy-MM-dd}"))
        {
            var items = group.ToList();
            for (var i = 0; i < items.Count; i++) for (var j = i + 1; j < items.Count; j++)
            {
                var left = items[i]; var right = items[j];
                var sameSummary = !string.IsNullOrWhiteSpace(left.Summary) && Normalize(left.Summary!) == Normalize(right.Summary ?? string.Empty);
                var sameParticipants = left.Men == right.Men && left.Women == right.Women && left.Boys == right.Boys && left.Girls == right.Girls && left.ElderlyMen == right.ElderlyMen && left.ElderlyWomen == right.ElderlyWomen && left.PersonsWithDisabilities == right.PersonsWithDisabilities;
                if (!sameSummary && !sameParticipants) continue;
                if (left.DuplicateStatus == "NoDuplicateDetected") left.DuplicateStatus = "PossibleDuplicate";
                if (right.DuplicateStatus == "NoDuplicateDetected") right.DuplicateStatus = "PossibleDuplicate";
                left.Warnings.Add("Possible duplicate: same site, activity type, date, and matching source evidence.");
                right.Warnings.Add("Possible duplicate: same site, activity type, date, and matching source evidence.");
            }
        }
        foreach (var group in records.Where(x => !string.IsNullOrWhiteSpace(x.ExternalRecordId)).GroupBy(x => x.ExternalRecordId!, StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1)) foreach (var item in group) item.DuplicateStatus = "ExactSourceDuplicate";
    }

    private static Dictionary<string, int> HeaderMap(IXLRangeRow row) => row.Cells().Select((cell, index) => new { Name = Normalize(cell.GetString()), Index = index + 1 }).Where(x => !string.IsNullOrWhiteSpace(x.Name)).GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x.First().Index);
    private static string Normalize(string value) => new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
    private static int? ParseNullableInt(string value) => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) && result >= 0 ? result : null;
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

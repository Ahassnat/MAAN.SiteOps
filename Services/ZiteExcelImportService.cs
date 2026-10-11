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
            result.DuplicateReviewGroups = BuildDuplicateReviewGroups(records);
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
            result.PossibleDuplicateRecordCount = result.PossibleDuplicates;
            result.DuplicateGroupCount = result.DuplicateReviewGroups.Count;
            result.AlreadyImported = records.Count(x => x.DuplicateStatus == "AlreadyImported");
            result.RecordsWithWarnings = records.Count(x => x.Warnings.Count > 0);
            result.WarningSummary = BuildWarningSummary(records);
            var unresolvedSiteRecords = records.Where(x => !x.MatchedSiteId.HasValue).ToList();
            result.TotalUnresolvedActivityRecords = unresolvedSiteRecords.Count;
            result.UnresolvedSiteSummary = unresolvedSiteRecords
                .GroupBy(x => new { x.SourceSiteCode, x.SourceSiteName })
                .Select(group => new UnresolvedActivitySiteViewModel
                {
                    SiteCode = group.Key.SourceSiteCode,
                    SiteName = group.Key.SourceSiteName,
                    ActivityRecordCount = group.Count(),
                    EarliestActivityDate = group.Where(x => x.ActivityDate.HasValue).Min(x => x.ActivityDate),
                    LatestActivityDate = group.Where(x => x.ActivityDate.HasValue).Max(x => x.ActivityDate)
                })
                .OrderBy(x => x.SiteCode)
                .ThenBy(x => x.SiteName)
                .ToList();
            result.TotalUniqueUnresolvedSites = result.UnresolvedSiteSummary.Count;
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or InvalidDataException)
        {
            result.Error = "The uploaded file could not be read as a valid Excel workbook.";
        }
        return result;
    }

    public async Task<SiteListPreviewViewModel> PreviewSitesAsync(Stream workbook, string sourceSystemCode, string fileName, CancellationToken cancellationToken = default)
    {
        var result = new SiteListPreviewViewModel { FileName = fileName };
        try
        {
            using var package = new XLWorkbook(workbook);
            var sheet = package.Worksheets.FirstOrDefault(x => Normalize(x.Name) == "sitelist");
            if (sheet is null) { result.Error = "The workbook does not contain a Site List sheet."; return result; }
            var used = sheet.RangeUsed();
            if (used is null) { result.Error = "The Site List sheet is empty."; return result; }

            var headers = HeaderMap(used.FirstRow());
            var siteCodeColumn = FindHeader(headers, "Site ID", "Site Code", "SiteCode");
            var siteNameColumn = FindHeader(headers, "Site Name", "SiteName");
            if (siteCodeColumn == 0 || siteNameColumn == 0)
            {
                result.Error = "The Site List sheet must include Site ID/Site Code and Site Name headers.";
                return result;
            }

            var sites = await _context.Sites.AsNoTracking().ToListAsync(cancellationToken);
            var governorates = await _context.Governorates.AsNoTracking().ToListAsync(cancellationToken);
            foreach (var row in used.RowsUsed().Skip(1))
            {
                string Value(params string[] names)
                {
                    var column = FindHeader(headers, names);
                    return column == 0 ? string.Empty : row.Cell(column).GetString().Trim();
                }

                var code = Value("Site ID", "Site Code", "SiteCode");
                var name = Value("Site Name", "SiteName");
                var sourceGovernorate = Value("region_1_name", "Governorate", "Governorate Name");
                var area = Value("region_2_name", "Area", "Municipality", "Locality");
                var siteLocation = Value("Site Location", "Location", "Coordinates");
                var (pointLatitude, pointLongitude, invalidPoint) = ParsePoint(siteLocation);
                var latitude = ParseDecimal(Value("Latitude", "Lat")) ?? pointLatitude;
                var longitude = ParseDecimal(Value("Longitude", "Long", "Lng")) ?? pointLongitude;
                var existing = sites.FirstOrDefault(site => !string.IsNullOrWhiteSpace(code) && string.Equals(site.SiteCode, code, StringComparison.OrdinalIgnoreCase));
                var matchedGovernorate = ResolveGovernorate(sourceGovernorate, governorates);
                var preview = new SiteListPreviewRecordViewModel
                {
                    RowNumber = row.RowNumber(),
                    SourceSiteCode = code,
                    SourceSiteName = name,
                    SourceGovernorate = sourceGovernorate,
                    MatchedGovernorateId = matchedGovernorate?.Id,
                    MatchedGovernorateName = matchedGovernorate?.Name,
                    SourceArea = area,
                    Latitude = latitude,
                    Longitude = longitude,
                    SourceStatus = Value("site_status", "Status", "Site Status", "Active Status"),
                    SourceAgency = Value("site_agency", "Agency"),
                    ExistingSiteId = existing?.Id
                };

                if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                {
                    preview.Status = "Error";
                    preview.IsError = true;
                    preview.Warnings.Add(string.IsNullOrWhiteSpace(code) ? "SiteCode is missing." : "Site Name is missing.");
                }
                else if (existing is null)
                {
                    preview.IsNew = true;
                    if (matchedGovernorate is null)
                    {
                        preview.IsGovernorateUnresolved = !string.IsNullOrWhiteSpace(sourceGovernorate);
                        preview.IsError = true;
                        preview.Warnings.Add("Governorate could not be resolved to an existing Governorate; new Site cannot be imported without it.");
                        preview.Status = "Error";
                    }
                    else
                    {
                        preview.Status = "New";
                    }
                }
                else if (matchedGovernorate is null)
                {
                    preview.IsGovernorateUnresolved = !string.IsNullOrWhiteSpace(sourceGovernorate);
                    preview.HasChanges = HasSiteChanges(existing, preview);
                    preview.Status = preview.HasChanges ? "Existing With Changes" : "Warning";
                    preview.Warnings.Add("Governorate could not be resolved; existing Governorate will be preserved.");
                }
                else
                {
                    preview.HasChanges = HasSiteChanges(existing, preview);
                    preview.Status = preview.HasChanges ? "Existing With Changes" : "Existing";
                    if (!string.Equals(existing.SiteName, name, StringComparison.Ordinal))
                        preview.Warnings.Add($"Site name differs: existing '{existing.SiteName}', source '{name}'.");
                }
                if (invalidPoint)
                    preview.Warnings.Add("Site Location is not a valid POINT(longitude latitude) value.");
                result.Records.Add(preview);
            }

            result.SitesRead = result.Records.Count;
            result.Errors = result.Records.Count(x => x.IsError);
            result.UnresolvedGovernorates = result.Records.Count(x => x.IsGovernorateUnresolved);
            result.NewSites = result.Records.Count(x => x.IsNew);
            result.ExistingSites = result.Records.Count(x => x.ExistingSiteId.HasValue);
            result.SitesWithChanges = result.Records.Count(x => x.ExistingSiteId.HasValue && x.HasChanges);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or InvalidDataException)
        {
            result.Error = "The uploaded file could not be read as a valid Excel workbook.";
        }
        return result;
    }

    public async Task<int> ImportSitesAsync(SiteListPreviewViewModel preview, CancellationToken cancellationToken = default)
    {
        var governorates = await _context.Governorates.ToListAsync(cancellationToken);
        var imported = 0;
        foreach (var item in preview.Records.Where(x => !x.IsError && !string.IsNullOrWhiteSpace(x.SourceSiteCode) && !string.IsNullOrWhiteSpace(x.SourceSiteName)))
        {
            var site = await _context.Sites.FirstOrDefaultAsync(x => x.SiteCode == item.SourceSiteCode, cancellationToken);
            if (site is null)
            {
                if (!item.MatchedGovernorateId.HasValue) continue;
                site = new Site
                {
                    SiteCode = item.SourceSiteCode!,
                    SiteName = item.SourceSiteName!,
                    GovernorateId = item.MatchedGovernorateId.Value,
                    Area = EmptyToNull(item.SourceArea),
                    Latitude = item.Latitude,
                    Longitude = item.Longitude,
                    IsActive = true,
                    IsManagedByMaan = true
                };
                _context.Sites.Add(site);
                imported++;
                continue;
            }

            if (!string.IsNullOrWhiteSpace(item.SourceSiteName)) site.SiteName = item.SourceSiteName!;
            if (item.MatchedGovernorateId.HasValue) site.GovernorateId = item.MatchedGovernorateId.Value;
            if (!string.IsNullOrWhiteSpace(item.SourceArea)) site.Area = item.SourceArea;
            if (item.Latitude.HasValue) site.Latitude = item.Latitude;
            if (item.Longitude.HasValue) site.Longitude = item.Longitude;
            site.UpdatedAt = DateTime.UtcNow;
            imported++;
        }
        await _context.SaveChangesAsync(cancellationToken);
        return imported;
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
        var topic = Value("Topic", "Topics");
        var reformattedDateCell = GetCell(row, headers, "Activity date (reformatted)");
        var sourceDateCell = GetCell(row, headers, "Activity date", "Date");
        var dateCell = reformattedDateCell is not null && !reformattedDateCell.IsEmpty() && !string.IsNullOrWhiteSpace(reformattedDateCell.GetString())
            ? reformattedDateCell
            : sourceDateCell;
        var datePresent = dateCell is not null && !dateCell.IsEmpty() && !string.IsNullOrWhiteSpace(dateCell.GetString());
        var date = TryParseExcelDate(dateCell);
        var men = ParseNullableInt(Value("men"));
        var women = ParseNullableInt(Value("women"));
        var boys = ParseNullableInt(Value("boys"));
        var girls = ParseNullableInt(Value("girls"));
        var elderlyMen = ParseNullableInt(Value("elderly men"));
        var elderlyWomen = ParseNullableInt(Value("elderly women"));
        var pwd = ParseNullableInt(Value("PWds", "Persons With Disabilities"));
        var sourceTotal = ParseNullableInt(Value("total participants", "Total Participants"));
        var calculatedTotal = (men ?? 0) + (women ?? 0) + (boys ?? 0) + (girls ?? 0) + (elderlyMen ?? 0) + (elderlyWomen ?? 0);
        var item = new ImportPreviewRecordViewModel { RowNumber = rowNumber, ExternalRecordId = externalId, SourceSiteCode = sourceCode, SourceSiteName = sourceName, SourceActivityType = sourceType, SourceActivityCategory = sourceCategory, Agency = agency, SourceDonorLabel = donor, ActivityDate = date, SourceTotalParticipants = sourceTotal, ParticipantsTotal = calculatedTotal, Summary = summary, Notes = notes, SourceTopic = topic, Men = men, Women = women, Boys = boys, Girls = girls, ElderlyMen = elderlyMen, ElderlyWomen = elderlyWomen, PersonsWithDisabilities = pwd, RecordHash = Hash(string.Join("|", "ZITE", externalId, sourceCode, date?.ToString("yyyy-MM-dd"), sourceType, summary)) };
        if (sourceTotal.HasValue && sourceTotal.Value != calculatedTotal) item.Warnings.Add($"Source total participants ({sourceTotal}) differs from calculated total ({calculatedTotal}).");

        var site = sites.FirstOrDefault(x => !string.IsNullOrWhiteSpace(sourceCode) && Normalize(x.SiteCode) == Normalize(sourceCode));
        if (site is not null) { item.MatchedSiteId = site.Id; item.MatchedSiteDisplay = $"{site.SiteCode} - {site.SiteName}"; item.SiteMatchStatus = "Matched"; if (!string.IsNullOrWhiteSpace(sourceName) && Normalize(site.SiteName) != Normalize(sourceName)) { item.SiteMatchStatus = "NameMismatch"; item.Warnings.Add("Site name differs from the matched SiteCode."); } }
        else if (!string.IsNullOrWhiteSpace(sourceName)) { site = sites.FirstOrDefault(x => Normalize(x.SiteName) == Normalize(sourceName)); if (site is not null) { item.MatchedSiteId = site.Id; item.MatchedSiteDisplay = $"{site.SiteCode} - {site.SiteName}"; item.SiteMatchStatus = "NameMismatch"; item.Warnings.Add("Matched by exact site name because SiteCode was unavailable."); } }

        var alias = Normalize(sourceType) switch { "communityledprojectplans" => "communityledactivityplan", "communityledprojects" => "communityledactivity", "siteimprovementplans" => "siteimprovementplan", "siteimprovementscareandmaintenance" => "siteimprovementscaremaintenance", "safetyaudits" => "safetyaudit", "awarenesssessions" => "awarenesssession", _ => Normalize(sourceType) };
        var activityType = types.FirstOrDefault(x => Normalize(x.Name) == alias || Normalize(x.Code) == alias);
        if (activityType is not null) { item.MatchedActivityTypeId = activityType.Id; item.MatchedActivityTypeName = activityType.Name; item.MatchedActivityTypeRequiresParticipants = activityType.RequiresParticipants; item.ActivityTypeMatchStatus = alias == Normalize(sourceType) ? "Matched" : "AliasMatched"; }
        if (string.IsNullOrWhiteSpace(sourceType)) item.Warnings.Add("Activity Type is missing.");
        if (!datePresent) item.Warnings.Add("Activity Date is missing");
        else if (!date.HasValue) item.Warnings.Add("Activity Date could not be parsed");
        else if (date.Value.Date > DateTime.Today) item.Warnings.Add("Activity Date is in the future");
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
                if (!HasMeaningfulPossibleDuplicateEvidence(left, right)) continue;
                if (left.DuplicateStatus == "NoDuplicateDetected") left.DuplicateStatus = "PossibleDuplicate";
                if (right.DuplicateStatus == "NoDuplicateDetected") right.DuplicateStatus = "PossibleDuplicate";
                left.Warnings.Add("Possible duplicate: same site, activity type, date, and matching source evidence.");
                right.Warnings.Add("Possible duplicate: same site, activity type, date, and matching source evidence.");
            }
        }
        foreach (var group in records.Where(x => !string.IsNullOrWhiteSpace(x.ExternalRecordId)).GroupBy(x => x.ExternalRecordId!, StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1)) foreach (var item in group) item.DuplicateStatus = "ExactSourceDuplicate";
    }

    private static List<DuplicateReviewGroupViewModel> BuildDuplicateReviewGroups(List<ImportPreviewRecordViewModel> records)
    {
        var candidates = records
            .Where(x => x.DuplicateStatus is "PossibleDuplicate" or "ExactSourceDuplicate")
            .OrderBy(x => x.RowNumber)
            .ToList();
        var adjacency = Enumerable.Range(0, candidates.Count).ToDictionary(index => index, _ => new HashSet<int>());
        var evidenceByPair = new Dictionary<(int Left, int Right), HashSet<string>>();

        for (var leftIndex = 0; leftIndex < candidates.Count; leftIndex++)
        for (var rightIndex = leftIndex + 1; rightIndex < candidates.Count; rightIndex++)
        {
            var left = candidates[leftIndex];
            var right = candidates[rightIndex];
            var reasons = new HashSet<string>(StringComparer.Ordinal);
            var repeatedExternalId = !string.IsNullOrWhiteSpace(left.ExternalRecordId) &&
                                     string.Equals(left.ExternalRecordId, right.ExternalRecordId, StringComparison.OrdinalIgnoreCase);
            if (repeatedExternalId)
                reasons.Add("Repeated External Record ID");

            var sameCore = !string.IsNullOrWhiteSpace(left.SourceSiteCode) &&
                           !string.IsNullOrWhiteSpace(right.SourceSiteCode) &&
                           Normalize(left.SourceSiteCode) == Normalize(right.SourceSiteCode) &&
                           left.MatchedActivityTypeId.HasValue && left.MatchedActivityTypeId == right.MatchedActivityTypeId &&
                           left.ActivityDate.HasValue && left.ActivityDate == right.ActivityDate;
            if (sameCore)
            {
                reasons.Add("Same site + activity type + date");
                if (left.MatchedActivityTypeRequiresParticipants && HasPositiveParticipants(left) && HasPositiveParticipants(right) && SameParticipants(left, right))
                    reasons.Add("Identical positive participant values for a type requiring participants");
                if (IsMeaningfulSummary(left.Summary) && Normalize(left.Summary!) == Normalize(right.Summary ?? string.Empty) &&
                    !string.IsNullOrWhiteSpace(left.Notes) && !string.IsNullOrWhiteSpace(right.Notes) && Normalize(left.Notes!) == Normalize(right.Notes!))
                    reasons.Add("Same informative summary and non-empty notes");
                if (!string.IsNullOrWhiteSpace(left.SourceTopic) && !string.IsNullOrWhiteSpace(right.SourceTopic) && Normalize(left.SourceTopic!) == Normalize(right.SourceTopic!))
                    reasons.Add("Same non-empty topic");
            }

            var hasDetectorEvidence = repeatedExternalId || sameCore && HasMeaningfulPossibleDuplicateEvidence(left, right);
            if (!hasDetectorEvidence) continue;
            adjacency[leftIndex].Add(rightIndex);
            adjacency[rightIndex].Add(leftIndex);
            evidenceByPair[(leftIndex, rightIndex)] = reasons;
        }

        var groups = new List<DuplicateReviewGroupViewModel>();
        var visited = new HashSet<int>();
        foreach (var start in Enumerable.Range(0, candidates.Count))
        {
            if (visited.Contains(start) || adjacency[start].Count == 0) continue;
            var component = new List<int>();
            var pending = new Stack<int>();
            pending.Push(start);
            visited.Add(start);
            while (pending.Count > 0)
            {
                var current = pending.Pop();
                component.Add(current);
                foreach (var neighbor in adjacency[current])
                    if (visited.Add(neighbor)) pending.Push(neighbor);
            }

            var members = component.Select(index => candidates[index]).OrderBy(x => x.RowNumber).ToList();
            var reasonsForGroup = new HashSet<string>(StringComparer.Ordinal);
            foreach (var left in component)
            foreach (var right in adjacency[left].Where(right => right > left))
                if (evidenceByPair.TryGetValue((left, right), out var reasons)) reasonsForGroup.UnionWith(reasons);

            var groupId = $"DUP-{groups.Count + 1:000}";
            foreach (var member in members) member.DuplicateGroupId = groupId;
            groups.Add(new DuplicateReviewGroupViewModel
            {
                GroupId = groupId,
                SiteCode = members.Select(x => x.SourceSiteCode).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)),
                SiteName = members.Select(x => x.SourceSiteName).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)),
                ActivityType = members.Select(x => x.MatchedActivityTypeName ?? x.SourceActivityType).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)),
                ActivityDate = members.Select(x => x.ActivityDate).FirstOrDefault(x => x.HasValue),
                RecordCount = members.Count,
                Reason = string.Join("; ", reasonsForGroup.OrderBy(x => x)),
                HasExactSourceDuplicateEvidence = reasonsForGroup.Contains("Repeated External Record ID"),
                Records = members
            });
        }
        return groups;
    }

    private static bool SameParticipants(ImportPreviewRecordViewModel left, ImportPreviewRecordViewModel right) =>
        left.Men == right.Men && left.Women == right.Women && left.Boys == right.Boys && left.Girls == right.Girls &&
        left.ElderlyMen == right.ElderlyMen && left.ElderlyWomen == right.ElderlyWomen &&
        left.PersonsWithDisabilities == right.PersonsWithDisabilities;

    private static bool HasMeaningfulPossibleDuplicateEvidence(ImportPreviewRecordViewModel left, ImportPreviewRecordViewModel right)
    {
        var hasPositiveParticipantEvidence = left.MatchedActivityTypeRequiresParticipants &&
            HasPositiveParticipants(left) && HasPositiveParticipants(right) && SameParticipants(left, right);

        var sameMeaningfulSummaryAndNotes = IsMeaningfulSummary(left.Summary) &&
            Normalize(left.Summary!) == Normalize(right.Summary ?? string.Empty) &&
            !string.IsNullOrWhiteSpace(left.Notes) &&
            !string.IsNullOrWhiteSpace(right.Notes) &&
            Normalize(left.Notes!) == Normalize(right.Notes!);

        var sameTopic = !string.IsNullOrWhiteSpace(left.SourceTopic) &&
            !string.IsNullOrWhiteSpace(right.SourceTopic) &&
            Normalize(left.SourceTopic!) == Normalize(right.SourceTopic!);

        return hasPositiveParticipantEvidence || sameMeaningfulSummaryAndNotes || sameTopic;
    }

    private static bool HasPositiveParticipants(ImportPreviewRecordViewModel record) =>
        (record.Men ?? 0) > 0 || (record.Women ?? 0) > 0 || (record.Boys ?? 0) > 0 ||
        (record.Girls ?? 0) > 0 || (record.ElderlyMen ?? 0) > 0 || (record.ElderlyWomen ?? 0) > 0 ||
        (record.PersonsWithDisabilities ?? 0) > 0;

    private static bool IsMeaningfulSummary(string? summary)
    {
        if (string.IsNullOrWhiteSpace(summary)) return false;
        var normalized = Normalize(summary);
        if (normalized.Length < 24) return false;
        string[] genericTokens = ["conducted", "held", "completed", "activity", "session", "meeting", "training", "audit"];
        var tokens = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var distinctMeaningfulTokens = tokens.Where(token => token.Length > 3 && !genericTokens.Contains(token, StringComparer.Ordinal)).Distinct().Count();
        return distinctMeaningfulTokens >= 3;
    }

    private static Dictionary<string, int> HeaderMap(IXLRangeRow row) => row.Cells().Select((cell, index) => new { Name = Normalize(cell.GetString()), Index = index + 1 }).Where(x => !string.IsNullOrWhiteSpace(x.Name)).GroupBy(x => x.Name).ToDictionary(x => x.Key, x => x.First().Index);
    private static IXLCell? GetCell(IXLRangeRow row, Dictionary<string, int> headers, params string[] names)
    {
        var column = names.Select(Normalize).Where(headers.ContainsKey).Select(name => headers[name]).FirstOrDefault();
        return column == 0 ? null : row.Cell(column);
    }

    private static DateTime? TryParseExcelDate(IXLCell? cell)
    {
        if (cell is null || cell.IsEmpty()) return null;
        if (cell.TryGetValue<DateTime>(out var dateTime)) return dateTime.Date;
        if (cell.TryGetValue<double>(out var serial) && serial >= 0)
        {
            try { return DateTime.FromOADate(serial).Date; }
            catch (ArgumentException) { return null; }
        }

        var text = cell.GetString().Trim();
        string[] formats = ["dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "yyyy-MM-dd"];
        return DateTime.TryParseExact(text, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed.Date
            : null;
    }

    private static List<ImportWarningSummaryViewModel> BuildWarningSummary(IEnumerable<ImportPreviewRecordViewModel> records)
    {
        var recordList = records.ToList();
        var summary = new Dictionary<string, int>(StringComparer.Ordinal);
        void Add(string reason, int count) { if (count > 0) summary[reason] = count; }
        Add("Activity Date is missing", recordList.Count(x => x.Warnings.Contains("Activity Date is missing")));
        Add("Activity Date could not be parsed", recordList.Count(x => x.Warnings.Contains("Activity Date could not be parsed")));
        Add("Activity Date is in the future", recordList.Count(x => x.Warnings.Contains("Activity Date is in the future")));
        Add("Participant total mismatch", recordList.Count(x => x.Warnings.Any(w => w.StartsWith("Source total participants", StringComparison.Ordinal))));
        Add("Site name mismatch", recordList.Count(x => x.Warnings.Any(w => w.Contains("Site name differs", StringComparison.Ordinal))));
        Add("Possible duplicate", recordList.Count(x => x.DuplicateStatus == "PossibleDuplicate"));
        var categorized = summary.Values.Sum();
        Add("Other", recordList.Sum(x => x.Warnings.Count) - categorized);
        return summary.Select(x => new ImportWarningSummaryViewModel { Reason = x.Key, Count = x.Value }).ToList();
    }
    private static int FindHeader(Dictionary<string, int> headers, params string[] names) => names.Select(Normalize).Where(headers.ContainsKey).Select(name => headers[name]).FirstOrDefault();
    private static Governorate? ResolveGovernorate(string source, List<Governorate> governorates)
    {
        var normalized = Normalize(source);
        var canonical = normalized switch
        {
            "deiralbalah" => "deiralbalah",
            "khanyunis" or "khanyounis" => "khanyounis",
            _ => normalized
        };
        return governorates.FirstOrDefault(x => Normalize(x.Name) == canonical);
    }

    private static (decimal? Latitude, decimal? Longitude, bool Invalid) ParsePoint(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return (null, null, false);
        var match = System.Text.RegularExpressions.Regex.Match(value.Trim(), @"^POINT\s*\(\s*([-+]?\d+(?:\.\d+)?)\s+([-+]?\d+(?:\.\d+)?)\s*\)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!match.Success) return (null, null, true);
        if (!decimal.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude) ||
            !decimal.TryParse(match.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude))
            return (null, null, true);
        return (latitude, longitude, false);
    }
    private static bool HasSiteChanges(Site site, SiteListPreviewRecordViewModel preview) =>
        !string.Equals(site.SiteName, preview.SourceSiteName, StringComparison.Ordinal) ||
        preview.MatchedGovernorateId.HasValue && site.GovernorateId != preview.MatchedGovernorateId ||
        !string.IsNullOrWhiteSpace(preview.SourceArea) && !string.Equals(site.Area, preview.SourceArea, StringComparison.Ordinal) ||
        preview.Latitude.HasValue && site.Latitude != preview.Latitude ||
        preview.Longitude.HasValue && site.Longitude != preview.Longitude;
    private static string? EmptyToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
    private static decimal? ParseDecimal(string value) => decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : null;
    private static string Normalize(string value) => new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
    private static int? ParseNullableInt(string value) => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) && result >= 0 ? result : null;
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

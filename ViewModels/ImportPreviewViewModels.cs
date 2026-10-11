namespace MAAN.SiteOps.ViewModels;

public class ImportPreviewViewModel
{
    public string? SourceSystemCode { get; set; }
    public string? FileName { get; set; }
    public string? Error { get; set; }
    public string? DetectedSheet { get; set; }
    public int RecordsRead { get; set; }
    public int ReadyForImport { get; set; }
    public int UnresolvedSites { get; set; }
    public int UnresolvedActivityTypes { get; set; }
    public int PossibleDuplicates { get; set; }
    public int ExactSourceDuplicates { get; set; }
    public int ActivityTypesMatched { get; set; }
    public int ActivityTypesUnresolved { get; set; }
    public IReadOnlyList<string> UnresolvedSourceActivityTypes { get; set; } = [];
    public int AlreadyImported { get; set; }
    public int RecordsWithWarnings { get; set; }
    public int BatchId { get; set; }
    public List<ImportPreviewRecordViewModel> Records { get; set; } = [];
}

public class ImportPreviewRecordViewModel
{
    public int RowNumber { get; set; }
    public string? ExternalRecordId { get; set; }
    public string? SourceSiteCode { get; set; }
    public string? SourceSiteName { get; set; }
    public int? MatchedSiteId { get; set; }
    public string? MatchedSiteDisplay { get; set; }
    public string? SourceActivityType { get; set; }
    public string? SourceActivityCategory { get; set; }
    public string? Agency { get; set; }
    public int? MatchedActivityTypeId { get; set; }
    public string? MatchedActivityTypeName { get; set; }
    public DateTime? ActivityDate { get; set; }
    public string? SourceDonorLabel { get; set; }
    public int ParticipantsTotal { get; set; }
    public int? SourceTotalParticipants { get; set; }
    public string SiteMatchStatus { get; set; } = "Unresolved";
    public string ActivityTypeMatchStatus { get; set; } = "Unresolved";
    public string DuplicateStatus { get; set; } = "NoDuplicateDetected";
    public string Status => DuplicateStatus is "AlreadyImported" or "PossibleDuplicate" or "ExactSourceDuplicate"
        ? "Duplicate" : SiteMatchStatus == "Unresolved" || ActivityTypeMatchStatus == "Unresolved" ? "Unresolved" : Warnings.Count > 0 ? "Warning" : "Ready";
    public List<string> Warnings { get; set; } = [];
    public bool ReadyForImport => MatchedSiteId.HasValue && MatchedActivityTypeId.HasValue && ActivityDate.HasValue && DuplicateStatus != "AlreadyImported";
    public string? RecordHash { get; set; }
    public string? Summary { get; set; }
    public string? Notes { get; set; }
    public int? Men { get; set; }
    public int? Women { get; set; }
    public int? Boys { get; set; }
    public int? Girls { get; set; }
    public int? ElderlyMen { get; set; }
    public int? ElderlyWomen { get; set; }
    public int? PersonsWithDisabilities { get; set; }
}
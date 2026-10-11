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
    public int PossibleDuplicateRecordCount { get; set; }
    public int DuplicateGroupCount { get; set; }
    public List<DuplicateReviewGroupViewModel> DuplicateReviewGroups { get; set; } = [];
    public int ActivityTypesMatched { get; set; }
    public int ActivityTypesUnresolved { get; set; }
    public IReadOnlyList<string> UnresolvedSourceActivityTypes { get; set; } = [];
    public int AlreadyImported { get; set; }
    public int RecordsWithWarnings { get; set; }
    public List<ImportWarningSummaryViewModel> WarningSummary { get; set; } = [];
    public int TotalUnresolvedActivityRecords { get; set; }
    public int TotalUniqueUnresolvedSites { get; set; }
    public List<UnresolvedActivitySiteViewModel> UnresolvedSiteSummary { get; set; } = [];
    public int BatchId { get; set; }
    public Guid SourceImportToken { get; set; }
    public int SourceRecordsImported { get; set; }
    public int SourceRecordsReady { get; set; }
    public int SourceRecordsNeedsReview { get; set; }
    public int SourceRecordsAlreadyImported { get; set; }
    public int SourceImportErrors { get; set; }
    public bool SourceImportCompleted { get; set; }
    public List<ImportPreviewRecordViewModel> Records { get; set; } = [];
}

public class UnresolvedActivitySiteViewModel
{
    public string? SiteCode { get; set; }
    public string? SiteName { get; set; }
    public int ActivityRecordCount { get; set; }
    public DateTime? EarliestActivityDate { get; set; }
    public DateTime? LatestActivityDate { get; set; }
}

public class DuplicateReviewGroupViewModel
{
    public string GroupId { get; set; } = string.Empty;
    public string? SiteCode { get; set; }
    public string? SiteName { get; set; }
    public string? ActivityType { get; set; }
    public DateTime? ActivityDate { get; set; }
    public int RecordCount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool HasExactSourceDuplicateEvidence { get; set; }
    public string ReviewClassification { get; set; } = "Needs Review";
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
    public bool MatchedActivityTypeRequiresParticipants { get; set; }
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
    public string? DuplicateGroupId { get; set; }
    public string? Summary { get; set; }
    public string? Notes { get; set; }
    public string? SourceTopic { get; set; }
    public int? Men { get; set; }
    public int? Women { get; set; }
    public int? Boys { get; set; }
    public int? Girls { get; set; }
    public int? ElderlyMen { get; set; }
    public int? ElderlyWomen { get; set; }
    public int? PersonsWithDisabilities { get; set; }
}

public class ImportWarningSummaryViewModel
{
    public string Reason { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class SourceRecordImportResultViewModel
{
    public int BatchId { get; set; }
    public int SourceRecordsRead { get; set; }
    public int SourceRecordsImported { get; set; }
    public int AlreadyImportedOrSkipped { get; set; }
    public int ExactSourceDuplicates { get; set; }
    public int ReadyForNormalization { get; set; }
    public int NeedsReview { get; set; }
    public int Errors { get; set; }
    public int ActivitiesCreated { get; set; }
}
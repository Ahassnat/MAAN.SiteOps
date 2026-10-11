namespace MAAN.SiteOps.ViewModels;

public class SiteListPreviewViewModel
{
    public Guid PreviewToken { get; set; }
    public int BatchId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? Error { get; set; }
    public int SitesRead { get; set; }
    public int NewSites { get; set; }
    public int ExistingSites { get; set; }
    public int SitesWithChanges { get; set; }
    public int UnresolvedGovernorates { get; set; }
    public int Errors { get; set; }
    public List<SiteListPreviewRecordViewModel> Records { get; set; } = [];
}

public class SiteListPreviewRecordViewModel
{
    public int RowNumber { get; set; }
    public string? SourceSiteCode { get; set; }
    public string? SourceSiteName { get; set; }
    public string? SourceGovernorate { get; set; }
    public int? MatchedGovernorateId { get; set; }
    public string? MatchedGovernorateName { get; set; }
    public string? SourceArea { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? SourceStatus { get; set; }
    public int? ExistingSiteId { get; set; }
    public string? SourceAgency { get; set; }
    public string Status { get; set; } = "Error";
    public List<string> Warnings { get; set; } = [];
    public bool HasChanges { get; set; }
    public bool IsNew { get; set; }
    public bool IsError { get; set; }
    public bool IsGovernorateUnresolved { get; set; }
}
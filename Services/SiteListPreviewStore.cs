using System.Collections.Concurrent;
using MAAN.SiteOps.ViewModels;

namespace MAAN.SiteOps.Services;

public sealed class SiteListPreviewStore
{
    private readonly ConcurrentDictionary<Guid, SiteListPreviewViewModel> _previews = new();
    private readonly ConcurrentDictionary<Guid, ActivitySourceImportPreview> _activityPreviews = new();

    public Guid Save(SiteListPreviewViewModel preview)
    {
        var token = Guid.NewGuid();
        preview.PreviewToken = token;
        _previews[token] = preview;
        return token;
    }

    public bool TryGet(Guid token, out SiteListPreviewViewModel? preview) => _previews.TryGetValue(token, out preview);

    public bool Remove(Guid token, out SiteListPreviewViewModel? preview) => _previews.TryRemove(token, out preview);

    public Guid SaveActivityPreview(ActivitySourceImportPreview preview)
    {
        var token = Guid.NewGuid();
        _activityPreviews[token] = preview;
        return token;
    }

    public bool RemoveActivityPreview(Guid token, out ActivitySourceImportPreview? preview) => _activityPreviews.TryRemove(token, out preview);
}

public sealed record ActivitySourceImportPreview(int BatchId, int SourceSystemId, string FileName, List<ActivitySourcePreviewRow> Rows);

public sealed record ActivitySourcePreviewRow(
    int RowNumber,
    string? ExternalRecordId,
    string? RecordHash,
    string? SourceSiteCode,
    string? SourceSiteName,
    int? MatchedSiteId,
    string? SourceActivityType,
    int? MatchedActivityTypeId,
    DateTime? SourceDate,
    string? SourceDonorLabel,
    int? Men,
    int? Women,
    int? Boys,
    int? Girls,
    int? ElderlyMen,
    int? ElderlyWomen,
    int? PersonsWithDisabilities,
    int? SourceTotalParticipants,
    int ParticipantsTotal,
    string? Summary,
    string? Notes,
    string? Agency,
    string? ActivityCategory,
    string? Topic,
    List<string> Warnings,
    string DuplicateStatus,
    bool IsReady);
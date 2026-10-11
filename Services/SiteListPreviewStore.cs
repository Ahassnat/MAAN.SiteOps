using System.Collections.Concurrent;
using MAAN.SiteOps.ViewModels;

namespace MAAN.SiteOps.Services;

public sealed class SiteListPreviewStore
{
    private readonly ConcurrentDictionary<Guid, SiteListPreviewViewModel> _previews = new();

    public Guid Save(SiteListPreviewViewModel preview)
    {
        var token = Guid.NewGuid();
        preview.PreviewToken = token;
        _previews[token] = preview;
        return token;
    }

    public bool TryGet(Guid token, out SiteListPreviewViewModel? preview) => _previews.TryGetValue(token, out preview);

    public bool Remove(Guid token, out SiteListPreviewViewModel? preview) => _previews.TryRemove(token, out preview);
}
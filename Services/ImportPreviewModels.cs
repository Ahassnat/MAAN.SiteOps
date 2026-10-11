using MAAN.SiteOps.ViewModels;

namespace MAAN.SiteOps.Services;

public interface IDataImportService
{
    Task<ImportPreviewViewModel> PreviewAsync(Stream workbook, string sourceSystemCode, string fileName, CancellationToken cancellationToken = default);
    Task<SiteListPreviewViewModel> PreviewSitesAsync(Stream workbook, string sourceSystemCode, string fileName, CancellationToken cancellationToken = default);
    Task<int> ImportSitesAsync(SiteListPreviewViewModel preview, CancellationToken cancellationToken = default);
}
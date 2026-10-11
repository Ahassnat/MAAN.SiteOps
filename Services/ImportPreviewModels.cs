using MAAN.SiteOps.ViewModels;

namespace MAAN.SiteOps.Services;

public interface IDataImportService
{
    Task<ImportPreviewViewModel> PreviewAsync(Stream workbook, string sourceSystemCode, string fileName, CancellationToken cancellationToken = default);
}
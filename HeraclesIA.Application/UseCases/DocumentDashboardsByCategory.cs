using HeraclesIA.Application.Abstractions;
using HeraclesIA.Types.Common;
using HeraclesIA.Types.Dashboards;
using Microsoft.Extensions.Logging;

namespace HeraclesIA.Application.UseCases;

public sealed class DocumentDashboardsByCategory
{
    private readonly ILocalDbRepository _localDb;
    private readonly IDashboardDocumentationService _documentation;
    private readonly IDashboardPublisher _publisher;
    private readonly IOnBaseRepository _onBase;
    private readonly ILogger<DocumentDashboardsByCategory> _logger;

    public DocumentDashboardsByCategory(
        ILocalDbRepository localDb,
        IDashboardDocumentationService documentation,
        IDashboardPublisher publisher,
        IOnBaseRepository onBase,
        ILogger<DocumentDashboardsByCategory> logger)
    {
        _localDb = localDb ?? throw new ArgumentNullException(nameof(localDb));
        _documentation = documentation ?? throw new ArgumentNullException(nameof(documentation));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _onBase = onBase ?? throw new ArgumentNullException(nameof(onBase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DocBatchResult> ExecuteAsync(IReadOnlyList<int> categoryIds, CancellationToken ct = default)
    {
        if (categoryIds is null) throw new ArgumentNullException(nameof(categoryIds));

        var result = new DocBatchResult();

        foreach (var categoryId in categoryIds)
        {
            ct.ThrowIfCancellationRequested();

            List<int> dashboardIds;
            try
            {
                dashboardIds = await _localDb.GetPendingDashboardsByCategoryAsync(categoryId, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                result.Errores++;
                _logger.LogError(ex, "Error GetPendingDashboardsByCategoryAsync categoryId={CategoryId}", categoryId);
                continue;
            }

            foreach (var dashboardId in dashboardIds)
            {
                ct.ThrowIfCancellationRequested();
                result.Considerados++;

                try
                {
                    var alreadyHas = await _localDb.HasDependenciesAsync(dashboardId, ct).ConfigureAwait(false);
                    if (alreadyHas)
                    {
                        result.Existentes++;
                        continue;
                    }

                    var dashboards = await _documentation.DocumentarAsync(dashboardId, ct).ConfigureAwait(false);
                    if (dashboards is null || dashboards.Count == 0)
                    {
                        result.SinCambios++;
                        continue;
                    }

                    result.Nuevos += dashboards.Count;

                    IReadOnlyList<Page> pages = Array.Empty<Page>();
                    try
                    {
                        pages = await _publisher.PublishAsync(dashboards, ct).ConfigureAwait(false);
                        if (pages != null && pages.Count > 0) result.Publicados++;
                    }
                    catch (NotSupportedException nse)
                    {
                        result.PublicacionOmitida++;
                        _logger.LogWarning(nse, "Publisher no soportado. DashboardId={DashboardId}", dashboardId);
                    }

                    try
                    {
                        await _onBase.UpdateOperacionesAsync(dashboardId, pages ?? Array.Empty<Page>(), ct).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        result.ErroresSideEffects++;
                        _logger.LogError(ex, "Error UpdateOperacionesAsync dashboardId={DashboardId}", dashboardId);
                    }

                    try
                    {
                        await _onBase.UpdateDashboardStatusAsync(dashboardId.ToString(), ct).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        result.ErroresSideEffects++;
                        _logger.LogError(ex, "Error UpdateDashboardStatusAsync dashboardId={DashboardId}", dashboardId);
                    }
                }
                catch (Exception ex)
                {
                    result.Errores++;
                    _logger.LogError(ex, "Error procesando dashboardId={DashboardId}", dashboardId);
                }
            }
        }

        return result;
    }
}

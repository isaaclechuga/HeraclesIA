using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HeraclesIA.Application.Abstractions;
using HeraclesIA.Types.Common;
using HeraclesIA.Types.Dashboards;

namespace HeraclesIA.Application.UseCases
{
    /// <summary>
    /// Orquesta el flujo completo por lista de dashboards.
    /// Mantiene conteo consistente: Considerados vs Nuevos vs Existentes.
    /// </summary>
    public sealed class DocumentDashboardsById
    {
        private readonly ILocalDbRepository _localDb;
        private readonly IDashboardDocumentationService _documentationService;
        private readonly IDashboardPublisher _publisher;
        private readonly IOnBaseRepository _onBase;
        private readonly ILogger<DocumentDashboardsById> _logger;

        public DocumentDashboardsById(
            ILocalDbRepository localDb,
            IDashboardDocumentationService documentationService,
            IDashboardPublisher publisher,
            IOnBaseRepository onBase,
            ILogger<DocumentDashboardsById> logger)
        {
            _localDb = localDb ?? throw new ArgumentNullException(nameof(localDb));
            _documentationService = documentationService ?? throw new ArgumentNullException(nameof(documentationService));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _onBase = onBase ?? throw new ArgumentNullException(nameof(onBase));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<DocBatchResult> ExecuteAsync(
            IReadOnlyList<int> dashboardIds,
            CancellationToken ct = default)
        {
            if (dashboardIds == null) throw new ArgumentNullException(nameof(dashboardIds));

            var result = new DocBatchResult();

            foreach (var dashboardId in dashboardIds)
            {
                ct.ThrowIfCancellationRequested();
                result.Considerados++;

                try
                {
                    var alreadyHasDeps = await _localDb.HasDependenciesAsync(dashboardId, ct).ConfigureAwait(false);
                    if (alreadyHasDeps)
                    {
                        result.Existentes++;
                        continue;
                    }

                    var dashboards = await _documentationService.DocumentarAsync(dashboardId, ct).ConfigureAwait(false);
                    var dashboardsCount = dashboards?.Count ?? 0;

                    if (dashboardsCount == 0)
                    {
                        result.SinCambios++;
                        continue;
                    }

                    result.Nuevos += dashboardsCount;

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

            return result;
        }
    }
}

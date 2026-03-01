// HeraclesIA.Application/UseCases/AnalyzeAutofill.cs
using HeraclesIA.Application.Abstractions;
using HeraclesIA.Types.Autofill;
using HeraclesIA.Types.Confluence;
using HeraclesIA.Types.Dashboards;
using Microsoft.Extensions.Logging;

namespace HeraclesIA.Application.UseCases;

public sealed class AnalyzeAutofill
{
    private readonly IAutofillRepository _repo;
    private readonly IAutofillAnalyzerService _analyzer;
    private readonly IAutofillPublisher _publisher;
    private readonly ILogger<AnalyzeAutofill> _logger;

    public AnalyzeAutofill(
        IAutofillRepository repo,
        IAutofillAnalyzerService analyzer,
        IAutofillPublisher publisher,
        ILogger<AnalyzeAutofill> logger)
    {
        _repo = repo;
        _analyzer = analyzer;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<AnalyzeAutofillResult> ExecuteAsync(int autofillNum = 0, CancellationToken ct = default)
    {
        var result = new AnalyzeAutofillResult();

        IReadOnlyList<AutoFill> autofills;
        try
        {
            autofills = await _repo.GetPendingAsync(autofillNum, ct);
        }
        catch (Exception ex)
        {
            result.Errores++;
            _logger.LogError(ex, "Error GetPendingAsync(autofillNum={AutofillNum})", autofillNum);
            return result;
        }

        foreach (var autofill in autofills)
        {
            ct.ThrowIfCancellationRequested();
            result.Considerados++;

            try
            {
                // Equivalente al viejo: obtener unity script por UnityScriptId y unir por seqnum
                var scripts = await _repo.GetUnityScriptsAsync(autofill.UnityScriptId, ct);
                var joinedCode = string.Join("", scripts.OrderBy(s => s.seqnum).Select(s => s.unitysourcetext ?? ""));

                if (string.IsNullOrWhiteSpace(joinedCode))
                {
                    result.Omitidos++;
                    continue;
                }

                // Equivalente al viejo: Gemini ExternalReference => JSON => References
                var references = await _analyzer.AnalyzeExternalReferencesAsync(joinedCode, ct);

                // Publicar (si quieres 100% igual al viejo cuando estaba comentado, apágalo desde config)
                IReadOnlyList<Page> pages = Array.Empty<Page>();
                try
                {
                    pages = await _publisher.PublishAsync(autofill, joinedCode, references, ct);
                    if (pages.Count > 0) result.Publicados++;
                }
                catch (NotSupportedException nse)
                {
                    result.PublicacionOmitida++;
                    _logger.LogWarning(nse, "Publisher no soportado. AutofillId={AutofillId}", autofill.AutofillId);
                }

                // Marcar procesado (equivalente a cambiar estatus, tú defines la query)
                try
                {
                    await _repo.MarkAsProcessedAsync(autofill.AutofillId, ct);
                    result.Procesados++;
                }
                catch (Exception ex)
                {
                    result.ErroresSideEffects++;
                    _logger.LogError(ex, "Error MarkAsProcessedAsync autofillId={AutofillId}", autofill.AutofillId);
                }
            }
            catch (Exception ex)
            {
                result.Errores++;
                _logger.LogError(ex, "Error analizando autofillId={AutofillId}", autofill.AutofillId);
            }
        }

        return result;
    }
}

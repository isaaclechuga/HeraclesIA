using HeraclesIA.Application.Abstractions;
using HeraclesIA.Application.Mapping;
using HeraclesIA.Types.Dashboards;
using Newtonsoft.Json;

namespace HeraclesIA.Application.Services;

public sealed class DashboardDocumentationService : IDashboardDocumentationService
{
    private readonly DashboardExtractionService _extractor;
    private readonly DashboardDetailMapper _mapper;
    private readonly ILocalDbRepository _localDb;
    private readonly IFileStore _files;

    public DashboardDocumentationService(
        DashboardExtractionService extractor,
        DashboardDetailMapper mapper,
        ILocalDbRepository localDb,
        IFileStore files)
    {
        _extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _localDb = localDb ?? throw new ArgumentNullException(nameof(localDb));
        _files = files ?? throw new ArgumentNullException(nameof(files));
    }

    public async Task<IReadOnlyList<Dashboard>> DocumentarAsync(int dashboardId, CancellationToken ct = default)
        => await Documentar(dashboardId, ct).ConfigureAwait(false);

    private async Task<List<Dashboard>> Documentar(int dashboardNum, CancellationToken ct)
    {
        var exists = await _localDb.HasDependenciesAsync(dashboardNum, ct).ConfigureAwait(false);

        if (!exists)
        {
            var dashboards = await _extractor.ExtractAsync(dashboardNum, 0, ct).ConfigureAwait(false);
            var detalles = _mapper.MapToDetalles(dashboards);

            foreach (var item in dashboards)
            {
                string jsonNewtonsoft = JsonConvert.SerializeObject(item, Formatting.Indented);
                var fileName = $"{item.DashboardNum}.{CleanFileName(item.DashboardName.Trim())}.json";
                await _files.SaveTextAsync($"dashboarddependencias/{fileName}", jsonNewtonsoft, ct).ConfigureAwait(false);
            }

            await _localDb.SaveDependenciesAsync(detalles, ct).ConfigureAwait(false);
            return dashboards.ToList();
        }

        return new List<Dashboard>();
    }

    private static string CleanFileName(string fileName)
        => string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
}

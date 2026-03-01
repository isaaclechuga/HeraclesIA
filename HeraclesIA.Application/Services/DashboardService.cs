using HeraclesIA.Application.Abstractions;
using HeraclesIA.Types.Dashboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.Application.Services
{
    public sealed class DashboardService : IDashboardService
    {
        private readonly DashboardExtractionService _extract;
        private readonly DashboardDocumentationService _doc;

        public DashboardService(DashboardExtractionService extract, DashboardDocumentationService doc)
        {
            _extract = extract;
            _doc = doc;
        }

        public Task<IReadOnlyList<Dashboard>> ExtractAsync(int dashboardNum = 0, int categoryNum = 0, CancellationToken ct = default)
            => _extract.ExtractAsync(dashboardNum, categoryNum, ct);

        public Task<string> DocumentAsync(Dashboard dashboard, CancellationToken ct = default)
            => Task.FromResult("Pendiente: documentar 1 dashboard (si lo ocupas lo armamos).");

        public Task<IReadOnlyList<string>> DocumentManyAsync(IEnumerable<Dashboard> dashboards, CancellationToken ct = default)
            => Task.FromResult((IReadOnlyList<string>)new List<string>());
    }
}

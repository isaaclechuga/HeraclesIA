using HeraclesIA.Types.Dashboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.Application.Abstractions
{
    public interface IDashboardService
    {
        Task<IReadOnlyList<Dashboard>> ExtractAsync(int dashboardNum = 0, int categoryNum = 0, CancellationToken ct = default);
        Task<string> DocumentAsync(Dashboard dashboard, CancellationToken ct = default);
        Task<IReadOnlyList<string>> DocumentManyAsync(IEnumerable<Dashboard> dashboards, CancellationToken ct = default);
    }
}

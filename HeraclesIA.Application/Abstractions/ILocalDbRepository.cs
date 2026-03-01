using HeraclesIA.Types.Dashboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeraclesIA.Application.Abstractions
{
    public interface ILocalDbRepository
    {
        Task<bool> HasDependenciesAsync(int dashboardId, CancellationToken ct = default);
        Task<List<int>> GetPendingDashboardsByCategoryAsync(int categoryNum, CancellationToken ct = default);
        Task SaveDependenciesAsync(IEnumerable<DashboardDetalle> dashboards, CancellationToken ct = default);
    }
}

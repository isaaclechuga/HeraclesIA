using HeraclesIA.Types.Dashboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.Application.Abstractions
{
    public interface IDashboardDocumentationService
    {
        Task<IReadOnlyList<Dashboard>> DocumentarAsync(int dashboardId, CancellationToken ct = default);
    }
}

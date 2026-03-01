using HeraclesIA.Types.Dashboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.Application.Abstractions
{
    public interface IDashboardPublisher
    {
        Task<IReadOnlyList<Page>> PublishAsync(IReadOnlyList<Dashboard> dashboards, CancellationToken ct = default);
    }
}

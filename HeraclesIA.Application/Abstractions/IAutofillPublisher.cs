using HeraclesIA.Types.Autofill;
using HeraclesIA.Types.Dashboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.Application.Abstractions
{
    public interface IAutofillPublisher
    {
        Task<IReadOnlyList<Page>> PublishAsync(AutoFill autofill, string code, References references, CancellationToken ct);
    }
}

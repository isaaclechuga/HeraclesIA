using HeraclesIA.Types.Autofill;
using HeraclesIA.Types.Dashboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.Application.Abstractions
{
    public interface IAutofillAnalyzerService
    {
        Task<References> AnalyzeExternalReferencesAsync(string code, CancellationToken ct);
    }
}

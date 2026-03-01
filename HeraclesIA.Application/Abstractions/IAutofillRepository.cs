using HeraclesIA.Types.Autofill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeraclesIA.Application.Abstractions
{
    public interface IAutofillRepository
    {
        Task<IReadOnlyList<AutoFill>> GetPendingAsync(int autofillNum, CancellationToken ct);
        Task<IReadOnlyList<UnityScript>> GetUnityScriptsAsync(int unityProjectNum, CancellationToken ct);
        Task MarkAsProcessedAsync(int autofillId, CancellationToken ct);
    }
}

using HeraclesIA.Types.Common;
using HeraclesIA.Types.Dashboards;

namespace HeraclesIA.Application.Abstractions
{
    public interface IOnBaseRepository
    {
        Task<IEnumerable<Dashboard>> ExtractQueriesAsync(int dashboardNum = 0, int categoryNum = 0, CancellationToken ct = default);
        Task UpdateDashboardStatusAsync(string dashboardNum, CancellationToken ct = default);
        Task UpdateOperacionesAsync(int dashboardNum, IReadOnlyList<Page> pages, CancellationToken ct = default);
        Task<IReadOnlyList<HelperEntity>> GetProcessDataAsync(int dashboardNum = 0, CancellationToken ct = default);
        Task SaveFolderIdAsync(IReadOnlyList<HelperEntity> dataList, CancellationToken ct = default);
        Task<string> GetStoreProceadureContentAsync(string dashSource, string storedProceadureName, CancellationToken ct = default);
    }
}

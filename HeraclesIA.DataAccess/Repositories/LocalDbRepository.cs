using HeraclesIA.Application.Abstractions;
using HeraclesIA.DataBase.Abstractions;
using HeraclesIA.Types.Dashboards;
using System.Data.Common;

namespace HeraclesIA.DataAccess.Repositories;

public sealed class LocalDbRepository : ILocalDbRepository
{
    private readonly IMySqlConnectionFactory _mySql;

    public LocalDbRepository(IMySqlConnectionFactory mySql)
    {
        _mySql = mySql ?? throw new ArgumentNullException(nameof(mySql));
    }

    public async Task<bool> HasDependenciesAsync(int dashboardId, CancellationToken ct = default)
    {
        var query = @"SELECT COUNT(*) FROM dashboard d
	                        INNER JOIN dashboarddependencias dd ON d.dashboardId = dd.dashboardId
                           WHERE d.dashboardId = @dashboardId";

        await using var cn = _mySql.CreatePruebasDb();
        await cn.OpenAsync(ct).ConfigureAwait(false);

        await using var cmd = cn.CreateCommand();
        cmd.CommandText = query;
        AddParam(cmd, "@dashboardId", dashboardId);

        var scalar = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
        return Convert.ToInt32(scalar) > 0;
    }

    public async Task<List<int>> GetPendingDashboardsByCategoryAsync(int categoryNum, CancellationToken ct = default)
    {
        var result = new List<int>();

        var query = @"SELECT d.dashboardId FROM dashboard d
                            	LEFT JOIN dashboarddependencias dd ON d.dashboardId = dd.dashboardId
                            WHERE d.CategoriaId = @categoryNum AND d.Estatus = true AND dd.dependenciaId IS NULL";

        await using var cn = _mySql.CreatePruebasDb();
        await cn.OpenAsync(ct).ConfigureAwait(false);

        await using var cmd = cn.CreateCommand();
        cmd.CommandText = query;
        AddParam(cmd, "@categoryNum", categoryNum);

        await using var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await rd.ReadAsync(ct).ConfigureAwait(false))
            result.Add(Convert.ToInt32(rd["dashboardId"]));

        return result;
    }

    public async Task SaveDependenciesAsync(IEnumerable<DashboardDetalle> dashboards, CancellationToken ct = default)
    {
        var querySave = @"INSERT INTO pruebasdb.dashboarddependencias(dashboardId,dataProviderId,servidor,baseDatos,tabla) 
                              VALUES(@dashboardId,@dataProviderId,@servidor,@baseDatos,@tabla);";

        await using var cn = _mySql.CreatePruebasDb();
        await cn.OpenAsync(ct).ConfigureAwait(false);

        await using var cmd = cn.CreateCommand();
        cmd.CommandText = querySave;

        foreach (var d in dashboards)
        {
            cmd.Parameters.Clear();
            AddParam(cmd, "@dashboardId", d.DashboardId);
            AddParam(cmd, "@dataProviderId", d.DataProviderId);
            AddParam(cmd, "@servidor", d.Servidor);
            AddParam(cmd, "@baseDatos", d.BaseDatos);
            AddParam(cmd, "@tabla", d.Tabla);

            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }
    }

    private static void AddParam(DbCommand cmd, string name, object? value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(p);
    }
}

using HeraclesIA.Application.Abstractions;
using HeraclesIA.Types.Common;
using HeraclesIA.Types.Dashboards;
using HeraclesIA.DataBase.Abstractions;
using System.Data;
using System.Data.Common;

namespace HeraclesIA.DataAccess.Repositories;

public sealed class OnBaseRepository : IOnBaseRepository
{
    private readonly ISqlConnectionFactory _sql;

    public OnBaseRepository(ISqlConnectionFactory sql)
    {
        _sql = sql ?? throw new ArgumentNullException(nameof(sql));
    }

    public Task<IEnumerable<Dashboard>> ExtractQueriesAsync(int dashboardNum = 0, int categoryNum = 0, CancellationToken ct = default)
    {
        var dataList = GetDashboardInfo(dashboardNum, categoryNum);
        var resultList = new List<Dashboard>();

        foreach (var dashboard in dataList.GroupBy(x => x.DashboardNum))
        {
            var item = new Dashboard
            {
                DashboardNum = dashboard.Key,
                DashboardName = dashboard.Select(c => c.DashboardName).FirstOrDefault() ?? string.Empty,
                Description = dashboard.Select(c => c.Description).FirstOrDefault() ?? string.Empty,
            };

            foreach (var dataProvider in dashboard.GroupBy(x => x.RptProviderNum))
            {
                var first = dataProvider.FirstOrDefault();
                if (first == null) continue;

                var dtProviderElement = new DataProvider
                {
                    DataProviderId = first.RptProviderNum,
                    DataSource = first.OdbcDataSource,
                    Name = first.RptDataProviderName,
                    HelpText = first.RptDataProviderHelpText
                };

                var joinedBlob = string.Join("", dataProvider.OrderBy(c => c.SeqNum).Select(x => x.SqlQuery));
                var queryJoined = Helper.convertToString(joinedBlob);

                dtProviderElement.Query = new Query(Types.Prompts.PromptType.SqlQuery, queryJoined);
                item.DataProviders.Add(dtProviderElement);
            }

            resultList.Add(item);
        }

        return Task.FromResult((IEnumerable<Dashboard>)resultList);
    }

    public Task<IReadOnlyList<HelperEntity>> GetProcessDataAsync(int dashboardNum = 0, CancellationToken ct = default)
        => Task.FromResult((IReadOnlyList<HelperEntity>)GetProcessData(dashboardNum).ToList());

    public Task SaveFolderIdAsync(IReadOnlyList<HelperEntity> dataList, CancellationToken ct = default)
    {
        SaveFolderId(dataList?.ToList() ?? new List<HelperEntity>());
        return Task.CompletedTask;
    }

    public Task UpdateDashboardStatusAsync(string dashboardNum, CancellationToken ct = default)
    {
        UpdateDashboardStatus(dashboardNum);
        return Task.CompletedTask;
    }

    public Task UpdateOperacionesAsync(int dashboardNum, IReadOnlyList<Page> pages, CancellationToken ct = default)
    {
        UpdateOperaciones(dashboardNum, pages?.ToList() ?? new List<Page>());
        return Task.CompletedTask;
    }

    public Task<string> GetStoreProceadureContentAsync(string dashSource, string storedProceadureName, CancellationToken ct = default)
        => Task.FromResult(GetStoreProceadureContent(dashSource, storedProceadureName));

    private IEnumerable<HelperEntity> GetProcessData(int databoardNum = 0)
    {
        var dataList = new List<HelperEntity>();

        const string query = @"
SELECT d.* 
FROM Catalogos.[dbo].[helper_Remediaciones] d
WHERE ((@dashboardNum IS NULL) OR (d.NewValue = @dashboardNum))
  AND ((@estatus IS NULL) OR (d.Estatus = @estatus))
ORDER BY FOLIO DESC";

        using var cnx = _sql.CreateCatalogos();
        cnx.Open();

        using var cmd = cnx.CreateCommand();
        cmd.CommandText = query;

        AddParam(cmd, "@dashboardNum", databoardNum == 0 ? DBNull.Value : databoardNum);
        AddParam(cmd, "@estatus", 0);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            dataList.Add(new HelperEntity
            {
                ProcessId = Convert.ToInt32(reader["ProcessId"]),
                CategoryNum = Convert.ToInt32(reader["Folio"]),
                CategoryName = Convert.ToString(reader["Keyword"]) ?? string.Empty,
                DashboardNum = Convert.ToString(reader["NewValue"]) ?? string.Empty,
                FolderId = Convert.ToString(reader["UdfField"]) ?? string.Empty,
                Estatus = Convert.ToBoolean(reader["Estatus"])
            });
        }

        return dataList;
    }

    private void SaveFolderId(List<HelperEntity> dataList)
    {
        if (dataList == null || dataList.Count == 0)
            return;

        const string query = @"
UPDATE Catalogos.[dbo].[helper_Remediaciones]
SET UdfField = @UdfField
WHERE Folio = @folio";

        using var cnx = _sql.CreateCatalogos();
        cnx.Open();

        using var cmd = cnx.CreateCommand();
        cmd.CommandText = query;

        foreach (var customEntity in dataList)
        {
            cmd.Parameters.Clear();
            AddParam(cmd, "@UdfField", customEntity.FolderId);
            AddParam(cmd, "@folio", customEntity.CategoryNum);
            cmd.ExecuteNonQuery();
        }
    }

    private void UpdateDashboardStatus(string dashboardNum)
    {
        const string query = @"
UPDATE Catalogos.[dbo].[helper_Remediaciones]
SET Estatus = 1
WHERE NewValue = @dashboardNum";

        using var cnx = _sql.CreateCatalogos();
        cnx.Open();

        using var cmd = cnx.CreateCommand();
        cmd.CommandText = query;

        AddParam(cmd, "@dashboardNum", dashboardNum);
        cmd.ExecuteNonQuery();
    }

    private void UpdateOperaciones(int dashboardNum, List<Page> pages)
    {
        if (pages == null || pages.Count == 0)
            return;

        const string query = @"
UPDATE Operaciones.[dbo].[hrc_Dashboards]
SET Documentacion = @documentacion
WHERE DashboardId = @dashboardId";

        using var cnx = _sql.CreateOperaciones();
        cnx.Open();

        using var cmd = cnx.CreateCommand();
        cmd.CommandText = query;

        foreach (var page in pages)
        {
            cmd.Parameters.Clear();
            AddParam(cmd, "@documentacion", page.Url);
            AddParam(cmd, "@dashboardId", dashboardNum);
            cmd.ExecuteNonQuery();
        }
    }

    private string GetStoreProceadureContent(string dashSource, string storedProceadureName)
    {
        var result = new List<string>();

        var helpText = getHelpText(dashSource, storedProceadureName);
        var storeName = getStoreProcedureName(storedProceadureName);

        try
        {
            using var cnx = _sql.CreateOperaciones();
            cnx.Open();

            using var cmd = cnx.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = helpText;
            AddParam(cmd, "@objname", storeName);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                result.Add(Convert.ToString(reader["Text"]) ?? string.Empty);
        }
        catch
        {
        }

        return string.Join("", result);
    }

    private IEnumerable<Temp> GetDashboardInfo(int dashboardNum = 0, int categoryNum = 0)
    {
        var dataList = new List<Temp>();

        var query = @"
SELECT
    CASE WHEN c.categorynum IS NULL THEN 0 ELSE c.categorynum END categorynum
    ,Rtrim(c.categoryname) categoryname
    ,d.dashboardnum
    ,Rtrim(d.dashboardname) dashboardname
    ,Rtrim(d.description) description
    ,r.rptprovidernum
    ,Rtrim(r.rptdataprovidername) rptdataprovidername
    ,Rtrim(r.helptext) rptdataproviderhelpText
    ,CASE WHEN Rtrim(ec.odbcdatasource) IS NULL THEN 'Nautilus' ELSE Rtrim(ec.odbcdatasource) END odbcdatasource
    ,rp.seqnum
    ,rp.obblobdata
FROM hsi.dashboardinfo d with(nolock)
    LEFT JOIN dashboardxcategory dc WITH(NOLOCK) ON d.dashboardnum = dc.dashboardnum
    LEFT JOIN hsi.dashboardcategory c WITH(NOLOCK) ON dc.categorynum = c.categorynum
    LEFT JOIN hsi.rptdashboardxprovider rd WITH(NOLOCK) ON rd.dashboardnum = d.dashboardnum
    LEFT JOIN hsi.rptdataprovider r WITH(NOLOCK) ON r.rptprovidernum = rd.rptprovidernum
    LEFT JOIN hsi.rptdataproviderxmlblob rp WITH(NOLOCK) ON rp.obblobnum = r.obblobnum
    LEFT JOIN hsi.rptdataproviderprops dp WITH(NOLOCK) ON dp.rptprovidernum = r.rptprovidernum AND dp.propertytype = 3
    LEFT JOIN hsi.externaldbconfig ec WITH(NOLOCK) ON dp.propertyvalue = ec.externaldbcfgnum
WHERE ((@dashboardNum IS NULL) OR (d.dashboardnum = @dashboardnum)) 
    AND ((@categoryNum IS NULL) OR (c.categoryNum = @categoryNum))
    AND rp.seqnum IS NOT NULL
ORDER BY d.dashboardnum, r.rptprovidernum,r.obblobnum DESC, rp.seqnum  ASC";

        using var cnx = _sql.CreateNautilus();
        cnx.Open();

        using var cmd = cnx.CreateCommand();
        cmd.CommandText = query;

        AddParam(cmd, "@dashboardNum", dashboardNum == 0 ? DBNull.Value : dashboardNum);
        AddParam(cmd, "@categoryNum", categoryNum == 0 ? DBNull.Value : categoryNum);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            dataList.Add(new Temp
            {
                DashboardNum = Convert.ToInt32(reader["dashboardnum"]),
                CategoryNum = Convert.ToString(reader["categoryNum"]) ?? string.Empty,
                CategoryName = Convert.ToString(reader["categoryName"]) ?? string.Empty,
                DashboardName = Convert.ToString(reader["dashboardname"]) ?? string.Empty,
                Description = Convert.ToString(reader["description"]) ?? string.Empty,
                RptProviderNum = Convert.ToInt32(reader["rptprovidernum"]),
                RptDataProviderName = Convert.ToString(reader["rptdataprovidername"]) ?? string.Empty,
                RptDataProviderHelpText = Convert.ToString(reader["rptdataproviderhelpText"]) ?? string.Empty,
                OdbcDataSource = Convert.ToString(reader["odbcdatasource"]) ?? string.Empty,
                SeqNum = Convert.ToInt32(reader["seqnum"]),
                SqlQuery = Convert.ToString(reader["obblobdata"]) ?? string.Empty
            });
        }

        return dataList;
    }

    private string getHelpText(string dataSource, string storeProcedure)
    {
        if (storeProcedure.Contains("base_banco_reportes", StringComparison.InvariantCultureIgnoreCase))
            return "[RPRTSDB.P.GSLB].[base_banco_reportes].[dbo].sp_HelpText";

        if (storeProcedure.Contains("siglocc", StringComparison.InvariantCultureIgnoreCase))
            return "[sglodb.p.gslb].[Siglo].dbo.sp_HelpText";

        if (storeProcedure.Contains("[ONBSDB.P.GSLB].[Catalogos]"))
            return "[ONBSDB.P.GSLB].[Catalogos].[dbo].sp_HelpText";

        if (dataSource == "Nautilus" || dataSource == "OnBase")
            return "[ONBSDB.P.GSLB].[Nautilus].[dbo].sp_HelpText";

        if (dataSource == "Catalogos")
            return "[ONBSDB.P.GSLB].[Catalogos].[dbo].sp_HelpText";

        return "sp_helptext";
    }

    private string getStoreProcedureName(string storeProcedureName)
    {
        var spSplited = storeProcedureName.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (spSplited.Length > 0)
            return spSplited.Last().Trim().Replace("[", "").Replace("]", "");

        return storeProcedureName.Trim();
    }

    private static void AddParam(DbCommand cmd, string name, object? value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(p);
    }
}

internal static class Helper
{
    public static string convertToString(string query)
    {
        byte[] decodedBytes = Convert.FromBase64String(query);
        string decodedString = System.Text.Encoding.UTF8.GetString(decodedBytes);
        return obtenerEntre(decodedString, "<SqlText><![CDATA[", "]]></SqlText>");
    }

    private static string obtenerEntre(string texto, string inicio, string fin)
    {
        if (string.IsNullOrEmpty(texto) || string.IsNullOrEmpty(inicio) || string.IsNullOrEmpty(fin))
            throw new ArgumentException("Los parámetros no pueden ser nulos o vacíos.");

        int indiceInicio = texto.IndexOf(inicio);
        if (indiceInicio == -1) return string.Empty;
        indiceInicio += inicio.Length;

        int indiceFin = texto.IndexOf(fin, indiceInicio);
        if (indiceFin == -1) return string.Empty;

        return texto.Substring(indiceInicio, indiceFin - indiceInicio);
    }
}

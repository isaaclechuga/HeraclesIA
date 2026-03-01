using HeraclesIA.Application.Abstractions;
using HeraclesIA.Types.Dashboards;
using HeraclesIA.Types.Prompts;
using Newtonsoft.Json;

namespace HeraclesIA.Application.Services;

public sealed class DashboardExtractionService
{
    private readonly IOnBaseRepository _onBase;
    private readonly IGeminiClient _gemini;

    public DashboardExtractionService(IOnBaseRepository onBase, IGeminiClient gemini)
    {
        _onBase = onBase ?? throw new ArgumentNullException(nameof(onBase));
        _gemini = gemini ?? throw new ArgumentNullException(nameof(gemini));
    }

    public async Task<IReadOnlyList<Dashboard>> ExtractAsync(int dashboardNum = 0, int categoryNum = 0, CancellationToken ct = default)
    {
        var dashboards = (await _onBase.ExtractQueriesAsync(dashboardNum, categoryNum, ct).ConfigureAwait(false)).ToList();

        foreach (var dash in dashboards)
        {
            foreach (var dp in dash.DataProviders)
            {
                if (dp.Query == null)
                    continue;

                if (string.IsNullOrWhiteSpace(dp.Query.SqlQuery))
                {
                    var q = new Query(PromptType.SQlDocument, "DataProvider interno de SQL Document");
                    q.Servidores.Add(new Servidor
                    {
                        Nombre = "ONBSDB.P.GSLB",
                        BaseDatos = new List<BaseDatos>
                        {
                            new BaseDatos
                            {
                                Nombre = "Nautilus",
                                Tablas = new List<string>{ "No es posible identificar tablas" }
                            }
                        }
                    });
                    dp.Query = q;
                    continue;
                }

                dp.Query = await ExtractSqlTreeCodeAsync(dp.DataSource, dp.Query.SqlQuery, ct).ConfigureAwait(false);
            }
        }

        return dashboards;
    }

    private async Task<Query> ExtractSqlTreeCodeAsync(string dataSource, string queryJoined, CancellationToken ct)
    {
        var query = new Query(PromptType.SqlQuery, queryJoined);

        if (Helper.EsProcedimiento(queryJoined))
        {
            query.Tipo = PromptType.StoreProcedure;
            query.Children = await AnalizarSP(dataSource, queryJoined, ct).ConfigureAwait(false);
            return query;
        }

        var serversResult = await _gemini.Analizar(PromptType.SqlExtractServer, queryJoined, dataSource, ct).ConfigureAwait(false);
        var jsonCleaned = serversResult.ReplaceJsonCharacter();

        if (jsonCleaned.Any() && !jsonCleaned.Contains("false", StringComparison.InvariantCultureIgnoreCase))
            query.Servidores = JsonConvert.DeserializeObject<List<Servidor>>(jsonCleaned) ?? new List<Servidor>();

        query.Children = await AnalizarSP(dataSource, queryJoined, ct).ConfigureAwait(false);
        return query;
    }

    private async Task<List<Query>> AnalizarSP(string dataSource, string storeProcedureName, CancellationToken ct)
    {
        var children = new List<Query>();

        var spAnalysisResult = await _gemini.Analizar(PromptType.StoreProcedure, storeProcedureName, "", ct).ConfigureAwait(false);

        if (spAnalysisResult.Any() && !spAnalysisResult.Contains("false", StringComparison.InvariantCultureIgnoreCase))
        {
            var storeProcedureList = spAnalysisResult
                .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(element => element.Trim())
                .ToArray();

            foreach (var storeProcedure in storeProcedureList)
            {
                var q = new Query(PromptType.StoreProcedure, storeProcedure);
                var storeProcedureContent = await _onBase.GetStoreProceadureContentAsync(dataSource, storeProcedure, ct).ConfigureAwait(false);
                var inner = await ExtractSqlTreeCodeAsync(dataSource, storeProcedureContent, ct).ConfigureAwait(false);
                q.Children.Add(inner);
                children.Add(q);
            }
        }

        return children;
    }
}

internal static class Helper
{
    public static bool EsProcedimiento(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return false;

        string normalizado = sql.Trim().ToUpperInvariant();
        return normalizado.StartsWith("EXEC ") || normalizado.StartsWith("EXECUTE ") || normalizado.StartsWith("EXEC(");
    }
}

internal static class StringExtensions
{
    public static string ReplaceJsonCharacter(this string value)
        => (value ?? string.Empty).Replace("```json", "").Replace("```", "").Trim();
}

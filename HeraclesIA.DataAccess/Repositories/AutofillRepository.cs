// HeraclesIA.Infrastructure/Repositories/AutofillRepository.cs
using HeraclesIA.Application.Abstractions;
using HeraclesIA.DataBase.Connections;
using HeraclesIA.Types.Autofill;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using MySqlConnector;
using System.Data;

namespace HeraclesIA.DataAccess.Repositories;

public sealed class AutofillRepository : IAutofillRepository
{
    private readonly DbOptions _opt;

    public AutofillRepository(IOptions<DbOptions> opt)
    {
        _opt = opt.Value;
    }

    public async Task<IReadOnlyList<AutoFill>> GetPendingAsync(int autofillNum, CancellationToken ct)
    {
        var list = new List<AutoFill>();

        const string sql = """
            SELECT * 
            FROM autoFills
            WHERE estatus = 0
              AND ((@autofillId IS NULL) OR (AutofillId = @autofillId))
            ORDER BY autofillId DESC
        """;

        await using var cnx = new MySqlConnection(_opt.MySqlPruebasDb);
        await cnx.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, cnx);
        cmd.Parameters.AddWithValue("@autofillId", autofillNum == 0 ? DBNull.Value : autofillNum);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            list.Add(new AutoFill
            {
                AutofillId = Convert.ToInt32(reader["AutofillId"]),
                Nombre = Convert.ToString(reader["Nombre"]) ?? "",
                UnityScriptId = Convert.ToInt32(reader["UnityScriptId"]),
                JiraParentId = Convert.ToInt32(reader["JiraParentId"]),
                Estatus = Convert.ToBoolean(reader["Estatus"])
            });
        }

        return list;
    }

    public async Task<IReadOnlyList<UnityScript>> GetUnityScriptsAsync(int unityProjectNum, CancellationToken ct)
    {
        var list = new List<UnityScript>();

        const string sql = """
            SELECT
                upi.unityprojectnum,
                upi.unityprojectname,
                upi.unityprojectdesc,
                upi.lastmodified,
                upi.usernum,
                upi.currentversion,
                usd.unitysourcenum,
                usd.sourceversionnum,
                usd.seqnum,
                usd.unitysourcetext
            FROM hsi.unityprojectinfo upi WITH(NOLOCK)
                LEFT JOIN hsi.unitysrcxproject usp WITH(NOLOCK)
                    ON upi.unityprojectnum = usp.unityprojectnum AND upi.currentversion = usp.projectversionnum
                LEFT JOIN hsi.unitysourcedata usd WITH(NOLOCK)
                    ON usp.unitysourcenum = usd.unitysourcenum AND usp.sourceversionnum = usd.sourceversionnum
            WHERE ((@unityprojectnum IS NULL) OR (upi.unityprojectnum = @unityprojectnum))
            ORDER BY usd.seqnum DESC
        """;

        await using var cnx = new SqlConnection(_opt.NautilusSqlServer);
        await cnx.OpenAsync(ct);

        await using var cmd = new SqlCommand(sql, cnx);
        cmd.Parameters.AddWithValue("@unityprojectnum", unityProjectNum == 0 ? DBNull.Value : unityProjectNum);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            list.Add(new UnityScript
            {
                unityprojectnum = Convert.ToInt32(reader["unityprojectnum"]),
                unityprojectname = Convert.ToString(reader["unityprojectname"]) ?? "",
                unityprojectdesc = Convert.ToString(reader["unityprojectdesc"]) ?? "",
                lastmodified = Convert.ToDateTime(reader["lastmodified"]),
                usernum = Convert.ToInt32(reader["usernum"]),
                currentversion = Convert.ToInt32(reader["currentversion"]),
                unitysourcenum = Convert.ToInt32(reader["unitysourcenum"]),
                sourceversionnum = Convert.ToInt32(reader["sourceversionnum"]),
                seqnum = Convert.ToInt32(reader["seqnum"]),
                unitysourcetext = Convert.ToString(reader["unitysourcetext"]) ?? ""
            });
        }

        return list;
    }

    public async Task MarkAsProcessedAsync(int autofillId, CancellationToken ct)
    {
        // Ajusta al nombre real de tu columna/tabla si difiere
        const string sql = """
            UPDATE autoFills
               SET estatus = 1
             WHERE AutofillId = @id
        """;

        await using var cnx = new MySqlConnection(_opt.MySqlPruebasDb);
        await cnx.OpenAsync(ct);

        await using var cmd = new MySqlCommand(sql, cnx);
        cmd.Parameters.AddWithValue("@id", autofillId);

        await cmd.ExecuteNonQueryAsync(ct);
    }
}
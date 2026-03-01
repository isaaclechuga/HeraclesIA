using HeraclesIA.DataBase.Abstractions;
using HeraclesIA.DataBase.Settings;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data.Common;

namespace HeraclesIA.DataBase.Factories;

public sealed class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly SqlSettings _settings;

    public SqlConnectionFactory(IOptions<SqlSettings> options)
    {
        _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public DbConnection CreateNautilus()
        => new SqlConnection(_settings.NautilusConnectionString);

    public DbConnection CreateCatalogos()
        => new SqlConnection(_settings.CatalogosConnectionString);

    public DbConnection CreateOperaciones()
        => new SqlConnection(_settings.OperacionesConnectionString);
}

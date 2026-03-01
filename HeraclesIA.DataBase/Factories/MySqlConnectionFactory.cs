using HeraclesIA.DataBase.Abstractions;
using HeraclesIA.DataBase.Settings;
using Microsoft.Extensions.Options;
using MySqlConnector;
using System.Data.Common;

namespace HeraclesIA.DataBase.Factories;

public sealed class MySqlConnectionFactory : IMySqlConnectionFactory
{
    private readonly MySqlSettings _settings;

    public MySqlConnectionFactory(IOptions<MySqlSettings> options)
    {
        _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public DbConnection CreatePruebasDb()
        => new MySqlConnection(_settings.PruebasDbConnectionString);
}

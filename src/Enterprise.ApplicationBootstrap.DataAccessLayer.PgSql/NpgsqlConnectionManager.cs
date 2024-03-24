using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ApplicationBootstrap.DataAccessLayer.Repository;
using JetBrains.Annotations;
using Npgsql;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.PgSql;

/// <summary>
/// Implementation of <see cref="IConnectionFactory"/> for pg-sql.
/// </summary>
public class NpgsqlConnectionFactory([NotNull] string connectionString) : IConnectionFactory
{
    /// <inheritdoc />
    public DbConnection CreateAndOpenConnection()
    {
        var connection = CreateNewConnection();
        connection.Open();
        return connection;
    }

    /// <inheritdoc />
    public async Task<DbConnection> CreateAndOpenConnectionAsync(CancellationToken ct)
    {
        var connection = CreateNewConnection();
        await connection.OpenAsync(ct);
        return connection;
    }

    private DbConnection CreateNewConnection()
    {
        return new NpgsqlConnection(connectionString);
    }
}
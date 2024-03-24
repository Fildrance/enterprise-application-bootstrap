using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Repository;

/// <summary>
/// Factory for db-connections.
/// </summary>
/// <remarks>Replaces factory from System.Data.Entity for ease of testing.</remarks>
public interface IConnectionFactory
{
    /// <summary>
    /// Creates new DbConnection and leaves lifecycle management to caller.
    /// </summary>
    [NotNull]
    DbConnection CreateAndOpenConnection();

    /// <summary>
    /// Creates new DbConnection and leaves lifecycle management to caller.
    /// </summary>
    [NotNull, ItemNotNull]
    Task<DbConnection> CreateAndOpenConnectionAsync(CancellationToken ct);
}
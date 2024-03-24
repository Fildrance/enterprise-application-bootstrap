using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Repository;

/// <summary>
/// Default <see cref="IDbContextHelper"/> implementation.
/// </summary>
public class DbContextHelper : IDbContextHelper
{
    /// <inheritdoc />
    public void SetTransaction(DbContext dbContext, DbTransaction transaction)
    {
        dbContext.Database.UseTransaction(transaction);
    }

    /// <inheritdoc />
    public Task SetTransactionAsync(DbContext dbContext, DbTransaction transaction, CancellationToken ct)
    {
        return dbContext.Database.UseTransactionAsync(transaction, ct);
    }

    /// <inheritdoc />
    public void SetConnection(DbContext dbContext, DbConnection connection)
    {
        dbContext.Database.SetDbConnection(connection);
    }
}
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Repository;

/// <summary>
/// Helper for connection-/transaction-related manipulations with DbContext.
/// </summary>
/// <remarks>
/// Extracted as interface for improved testability.
/// </remarks>
public interface IDbContextHelper
{
    /// <summary>
    /// Sets <paramref name="transaction"/> as current transaction for connection underneath <paramref name="dbContext"/>.
    /// </summary>
    /// <param name="dbContext">DbContext to edit.</param>
    /// <param name="transaction">Transaction to be used.</param>
    void SetTransaction(DbContext dbContext, DbTransaction transaction);

    /// <summary>
    /// Sets <paramref name="transaction"/> as current transaction for connection underneath <paramref name="dbContext"/>.
    /// </summary>
    /// <param name="dbContext">DbContext to edit.</param>
    /// <param name="transaction">Transaction to be used.</param>
    /// <param name="ct">Token for operation cancellation.</param>
    Task SetTransactionAsync(DbContext dbContext, DbTransaction transaction, CancellationToken ct);

    /// <summary>
    /// Sets <paramref name="connection"/> as current connection underneath <paramref name="dbContext"/>.
    /// </summary>
    /// <param name="dbContext">DbContext to edit.</param>
    /// <param name="connection">Connection to be used.</param>
    void SetConnection(DbContext dbContext, DbConnection connection);
}
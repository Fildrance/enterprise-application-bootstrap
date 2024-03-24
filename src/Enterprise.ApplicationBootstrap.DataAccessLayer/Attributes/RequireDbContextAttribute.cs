using System;
using MediatR;
using IsolationLevel = System.Data.IsolationLevel;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Attributes;

/// <summary>
/// Attribute-marker for <see cref="EntityFrameworkManagingBehaviour{TRequest,TResponse}"/> to manage DbContext/connection/transaction(if needed).
/// When applied on type, derived from <see cref="IRequestHandler{TRequest}"/> or <see cref="IRequestHandler{TRequest,TResponse}"/> - 
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class RequireDbContextAttribute : Attribute
{
    /// <inheritdoc />
    public RequireDbContextAttribute()
    {
        WithTransactionIsolationLevel = null;
    }

    /// <inheritdoc />
    public RequireDbContextAttribute(IsolationLevel withTransactionIsolationLevel)
    {
        WithTransactionIsolationLevel = withTransactionIsolationLevel;
    }

    /// <summary>
    /// Isolation level for required transaction. If null - no transaction will be set.
    /// </summary>
    public IsolationLevel? WithTransactionIsolationLevel { get; set; }
}
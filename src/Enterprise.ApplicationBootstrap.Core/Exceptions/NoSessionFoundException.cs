using System;
using JetBrains.Annotations;

namespace Enterprise.ApplicationBootstrap.Core.Exceptions;

/// <summary>
/// Error due to no session found when expecting one.
/// </summary>
public class NoSessionFoundException : Exception
{
    /// <inheritdoc />
    public NoSessionFoundException() : base("Was expecting session object in local context (Session.Current AsyncLocal store) but found nont.")
    {
    }

    /// <inheritdoc />
    public NoSessionFoundException([CanBeNull] string message) : base(message)
    {
    }
}
using System;
using System.Collections.Generic;
using Enterprise.ApplicationBootstrap.DataAccessLayer.Configuration;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Repository;

/// <summary> Custom db-context that can be used in reusable way. Consumes all configurations that DI Container will pass.</summary>
public class CustomDbContext([NotNull] DbContextOptions options, [NotNull] IEnumerable<IContextConfiguration> configurations)
    : DbContext(options)
{
    private readonly IEnumerable<IContextConfiguration> _configurations = configurations ?? throw new ArgumentNullException(nameof(configurations));

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var config in _configurations)
        {
            config.Apply(modelBuilder);
        }
    }
}
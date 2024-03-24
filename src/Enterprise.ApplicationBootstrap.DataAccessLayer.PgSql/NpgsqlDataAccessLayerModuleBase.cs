using System;
using Enterprise.ApplicationBootstrap.Core;
using Enterprise.ApplicationBootstrap.DataAccessLayer.Modules;
using Enterprise.ApplicationBootstrap.DataAccessLayer.Repository;
using FluentMigrator.Runner;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.PgSql;

/// <summary> Module for pg sql adapter for EntityFramework.</summary>
/// <param name="connectionStringName">Name for connection string of database field in configuration under ConnectionStrings section.</param>
/// <param name="context">App initialization context.</param>
[PublicAPI]
public abstract class NpgsqlDataAccessLayerModuleBase(
    [NotNull] string connectionStringName,
    [NotNull] AppInitializationContext context
) : DataAccessLayerModuleBase(context)
{
    /// <summary> Marker for enabling EF logging. Currently registered <see cref="ILoggerFactory"/> will be used if enabed. </summary>
    public virtual bool IsEntityFrameworkLoggingEnabled => false;

    /// <inheritdoc />
    public override void Configure(IServiceCollection services, IConfiguration configuration)
    {
        base.Configure(services, configuration);

        var connectionString = configuration.GetConnectionString(connectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            const string message = $"Cannot initiate ef working with db, no connection string passed. {nameof(connectionStringName)} should be set on derived from "
                                   + $"{nameof(DataAccessLayerModuleBase)} type and this key should match some valid key in system configuration in 'ConnectionStrings' section.";
            throw new InvalidOperationException(message);
        }

        // Add common FluentMigrator services
        services.AddFluentMigratorCore()
                .ConfigureRunner(
                    rb => Configure(rb, connectionString)
                ).AddLogging(x => x.AddFluentMigratorConsole());

        services.AddDbContextFactory<CustomDbContext>(
            (sp, x) =>
            {
                x.UseNpgsql(connectionString, ConfigureNpgsqlOptions)
                 .UseSnakeCaseNamingConvention();
                if (IsEntityFrameworkLoggingEnabled)
                {
                    x.UseLoggerFactory(sp.GetRequiredService<ILoggerFactory>());
                }
            });
        services.TryAddSingleton<IConnectionFactory>(new NpgsqlConnectionFactory(connectionString));
    }


    /// <summary>
    /// Configures migration runner.
    /// </summary>
    /// <param name="builder">Builder for setting up runner.</param>
    /// <param name="connectionString">Connection string for database.</param>
    protected virtual void Configure(IMigrationRunnerBuilder builder, string connectionString)
    {
        var scanThisAssembly = GetType().Assembly;
        builder.ScanIn(scanThisAssembly).For.Migrations()
               .AddPostgres()
               .WithGlobalConnectionString(connectionString);
    }

    /// <summary>
    /// Override delegate for NpgsqlOptions configuration.
    /// </summary>
    protected virtual void ConfigureNpgsqlOptions([NotNull] NpgsqlDbContextOptionsBuilder dbContextOptionsBuilder)
    {
    }
}
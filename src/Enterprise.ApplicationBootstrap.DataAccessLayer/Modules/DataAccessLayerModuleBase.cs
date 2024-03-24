using System;
using AutoMapper;
using Enterprise.ApplicationBootstrap.Core;
using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using Enterprise.ApplicationBootstrap.DataAccessLayer.Configuration;
using Enterprise.ApplicationBootstrap.DataAccessLayer.Discover.EntityExtractor;
using Enterprise.ApplicationBootstrap.DataAccessLayer.Repository;
using FluentMigrator.Runner;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Modules;

/// <summary>
/// Base type for DataAccessLayer module. Registers migration logic, EF types, scans for repository-suppliers (such as <see cref="IFilterAdapterConcrete{TFilterContract,TEntity}"/> etc.
/// </summary>
public abstract class DataAccessLayerModuleBase(AppInitializationContext context) : IServiceCollectionAwareModule, IServiceProviderAwareModule
{
    private const string IsMigrationsEnabledFlagName = "IsMigrationsEnabled";

    /// <inheritdoc />
    public string ModuleIdentity => "DataAccessLayerModule";

    private readonly ILogger _logger = context.Logger;

    /// <inheritdoc />
    public virtual void Configure(IServiceCollection services, IConfiguration configuration)
    {
        var scanThisAssembly = GetType().Assembly;

        services.AddSingleton(typeof(IFilterAdapter<,>), typeof(GenericFilterAdapter<,>));

        services.Scan(
            x => x.FromAssemblies(scanThisAssembly)
                  .AddClasses(c => c.AssignableTo(typeof(IAdditionalExtractConfiguration<,>)).Where(t => t is { IsAbstract: false, IsGenericTypeDefinition: false }))
                  .AsImplementedInterfaces()
                  .AddClasses(c => c.AssignableTo(typeof(IFilterAdapterConcrete<,>)).Where(t => t is { IsAbstract: false, IsGenericTypeDefinition: false }))
                  .AsImplementedInterfaces()
                  .AddClasses(c => c.AssignableTo<IContextConfiguration>().Where(t => t is { IsAbstract: false, IsGenericTypeDefinition: false }))
                  .AsImplementedInterfaces()
                  .AddClasses(c => c.AssignableTo<Profile>().Where(t => t is { IsAbstract: false, IsGenericTypeDefinition: false }))
                  .As<Profile>()
        );
        services.TryAddSingleton<IDbContextManager, DbContextManager>();
        services.TryAddSingleton<IDbContextHelper, DbContextHelper>();
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(EntityFrameworkManagingBehaviour<,>));
    }

    /// <inheritdoc />
    public void Configure(IServiceProvider sp)
    {
        var isMigrationEnabled = sp.GetRequiredService<IConfiguration>().GetValue<bool>(IsMigrationsEnabledFlagName);
        if (!isMigrationEnabled)
        {
            _logger.LogInformation($"Skipping migration initialization - '{IsMigrationsEnabledFlagName}' is {false}.");
            return;
        }

        _logger.LogInformation($"Detected '{IsMigrationsEnabledFlagName}' is {true}.");
        using (var scope = sp.CreateScope())
        {
            // Instantiate the runner
            var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            // Execute the migrations
            runner.MigrateUp();
        }

        _logger.LogInformation($"Finished executing migraions.");
    }
}
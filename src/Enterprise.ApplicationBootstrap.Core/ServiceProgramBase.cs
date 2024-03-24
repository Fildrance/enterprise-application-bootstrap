using Enterprise.ApplicationBootstrap.Core.SystemEvents;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using System.Linq;
using Enterprise.ApplicationBootstrap.Core.Api;
using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using Enterprise.ApplicationBootstrap.Core.Initialization;
using Enterprise.ApplicationBootstrap.Logging.Serilog.Extension;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Enterprise.ApplicationBootstrap.Core.Exceptions;
using Enterprise.ApplicationBootstrap.Core.Extensions;
using Enterprise.ApplicationBootstrap.Core.HandlerMetadata;
using Enterprise.ApplicationBootstrap.Core.HealthChecks;
using Enterprise.ApplicationBootstrap.Core.Topology;
using Enterprise.ApplicationBootstrap.Core.Validation;
using FluentValidation;
using MediatR;
using Enterprise.ApplicationBootstrap.Core.Logging;
using Microsoft.Extensions.ObjectPool;

namespace Enterprise.ApplicationBootstrap.Core;

/// <summary>
/// Base type for service application (without need for listening for http).
/// </summary>
[PublicAPI]
public abstract class ServiceProgramBase
{
    static ServiceProgramBase()
    {
        Environment.CurrentDirectory = AppContext.BaseDirectory;

        try
        {
            Console.OutputEncoding = Encoding.UTF8;
        }
        //can fail for some cases of winservice
        catch (IOException)
        {
            // no-op
        }
    }

    /// <summary>
    /// Refines <see cref="hostBuilder"/> with common code - logging settings, telemetry, application name,
    /// initializable services etc. by using modules registered in <see cref="GetModules"/>.
    /// </summary>
    /// <param name="hostBuilder">Host to be refined.</param>
    /// <param name="commandLineArgs">Command line arguments from application launch.</param>
    /// <returns>Refined builder.</returns>
    [NotNull]
    protected IHostBuilder RefineBuilder([NotNull] IHostBuilder hostBuilder, [NotNull, ItemCanBeNull] string[] commandLineArgs)
    {
        hostBuilder.ConfigureAppConfiguration(
            (builderContext, builder) => SetupConfiguration(builder, builderContext.HostingEnvironment, commandLineArgs)
        );

        SetupLogging(hostBuilder);
        hostBuilder.ConfigureServices((builderContext, services) =>
        {
            services.AddLogging(builder => { });
            var applicationName = ApplicationName.BuildApplicationName(GetType());

            var appLogger = GetApplicationStartupLogger(services, builderContext.HostingEnvironment);

            var context = new AppInitializationContext(applicationName, builderContext.HostingEnvironment, builderContext.Configuration, appLogger);
            var modules = GetModules(context);
            ValidateModules(modules, appLogger);
            appLogger.LogConfigureStart(context.ApplicationName, modules);

            AddCommonServices(services, context);

            AddTelemetryInternal(services, context, modules);

            RegisterModuleServicesInternal(services, context, modules);

            RegisterModulesAsServices(services, modules, context);

            AddHealthChecksInternal(services, context, modules);
        });

        return hostBuilder;
    }

    private static void AddCommonServices(IServiceCollection services, AppInitializationContext context)
    {
        services.AddSingleton(context.ApplicationName);
        services.AddHostedService<ApplicationServicesInitializingBackgroundService>();
        services.AddSingleton<IOnApplicationStartInitializable, ApplicationLifetimeEventLogger>();
        context.Logger.LogInformation("Added ApplicationServicesInitializingBackgroundService to handle services marked with IOnApplicationStartInitializable.");

        services.TryAddSingleton<SystemEventsManager, LoggingSystemEventsManager>();
        services.TryAddSingleton<IValidatorProvider, ValidatorProvider>();
        services.Scan(
            x => x.FromApplicationDependencies()
                  .AddClasses(
                      c => c.AssignableTo(typeof(IValidator<>))
                            .Where(ci => !ci.IsAbstract
                                         && (!ci.IsGenericType || ci.IsConstructedGenericType)
                                         && (ci.Namespace?.StartsWith("System.") != true
                                             && ci.Namespace?.StartsWith("Microsoft.") != true)
                                         && ci.Name != nameof(CompositeValidator)
                            )
                  ).As(type =>
                  {
                      var validatorForTypeInterface = type.GetInterfaces()
                                                          .First(i => i.GetGenericTypeDefinition() == typeof(IValidator<>));
                      return new[] { validatorForTypeInterface };
                  }).As<IValidator>()
                  .WithSingletonLifetime()
        );
        services.AddSingleton<IMediator, Mediator>();
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ValidatingBehaviour<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(SessionInitializingBehaviour<,>));
        services.AddSingleton<IContractToLogConverterProvider, ContractToLogConverterProvider>();
        services.TryAddKeyedSingleton<IContractToLogConverter, ToStringContractToLogConverter>(IContractToLogConverter.DefaultContractToLogConverter);

        services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        services.TryAddSingleton(
            sp => sp.GetRequiredService<ObjectPoolProvider>()
                    .Create(new CollectionCleaningPoolPolicy())
        );

        var requestHandlerInterface = typeof(IRequestHandler<>);
        var requestHandlerInterface2 = typeof(IRequestHandler<,>);

        services.Scan(
            x => x.FromApplicationDependencies()
                  .AddClasses(
                      c => c.Where(cc =>
                      {
                          var interfaces = cc.GetInterfaces()
                                             .Where(i => i.IsConstructedGenericType)
                                             .Select(i => i.GetGenericTypeDefinition())
                                             .Select(@interface => @interface == requestHandlerInterface || @interface == requestHandlerInterface2);
                          return interfaces.FirstOrDefault();
                      })
                  ).AsImplementedInterfaces()
                  .WithSingletonLifetime()
        );


        context.Logger.LogInformation(
            "Added FluentValidator behaviour to Mediatr dispatcher. "
            + "All validators from EntryPoint Assembly and its dependencies "
            + "are going to be scanned and added for ValidatorProvider."
        );
    }

    /// <summary>
    /// Refines <see cref="hostBuilder"/> with common code - logging settings, telemetry, application name,
    /// initializable services etc. by using modules registered in <see cref="GetModules"/>.
    /// </summary>
    /// <param name="hostBuilder">Host to be refined.</param>
    /// <param name="commandLineArgs">Command line arguments from application launch.</param>
    /// <returns>Ready to start Host.</returns>
    [NotNull]
    protected IHost BuildApplication([NotNull] IHostBuilder hostBuilder, [NotNull, ItemCanBeNull] string[] commandLineArgs)
    {
        RefineBuilder(hostBuilder, commandLineArgs);
        var host = hostBuilder.Build();
        var applicationServices = host.Services;

        var context = applicationServices.GetRequiredService<AppInitializationContext>();
        var modules = applicationServices.GetRequiredService<IEnumerable<IModule>>()
                                         .ToArray();

        ExecuteServiceProviderAwareModules(applicationServices, context, modules);

        return host;
    }

    /// <summary> Sets up IConfiguration. </summary>
    protected virtual IConfigurationBuilder SetupConfiguration(
        [NotNull] IConfigurationBuilder configurationManager,
        [NotNull] IHostEnvironment hostEnvironment,
        [NotNull] [ItemNotNull] string[] commandLineArgs
    )
    {
        return configurationManager;
    }

    /// <summary> Sets up application logging. </summary>
    protected virtual void SetupLogging(
        [NotNull] IHostBuilder hostBuilder
    )
    {
        hostBuilder.UseCustomSerilog();
    }

    /// <summary> Gets logger for application initialization. </summary>
    /// <remarks>In case logging is not required, NullLogger can be used.</remarks>
    [NotNull]
    protected virtual ILogger GetApplicationStartupLogger(
        [NotNull] IServiceCollection services,
        [NotNull] IHostEnvironment hostEnvironment
    )
    {
        using var localProvider = services.BuildServiceProvider();
        var loggerType = typeof(ILogger<>).MakeGenericType(GetType());
        return (ILogger)localProvider.GetRequiredService(loggerType);
    }

    /// <summary> Returns list of registered modules. </summary>
    /// <param name="context">Application initialization context.</param>
    [NotNull, ItemNotNull]
    protected abstract IReadOnlyCollection<IModule> GetModules([NotNull] AppInitializationContext context);

    /// <summary> Gets invoker for telemetry setup. </summary>
    [NotNull]
    protected abstract TelemetrySetupInvokerBase GetInvoker();

    /// <summary>
    /// If true - turns warning about no health-checks registered off.
    /// </summary>
    protected bool SuppressNoHealthCheckWarning { get; set; }

    /// <summary> Executes logic of all modules which implements <see cref="IServiceProviderAwareModule"/>. </summary>
    /// <param name="applicationServices">DI Container.</param>
    /// <param name="modules">List of modules to be used.</param>
    /// <param name="appInitializationContext">Application initialization context.</param>
    protected static void ExecuteServiceProviderAwareModules(
        [NotNull] IServiceProvider applicationServices,
        [NotNull] AppInitializationContext appInitializationContext,
        [NotNull, ItemNotNull] IReadOnlyCollection<IModule> modules
    )
    {
        var systemEventsManager = applicationServices.GetRequiredService<SystemEventsManager>();
        systemEventsManager.Complete(SystemEventNames.ConfigurationComplete);

        modules.ForEachOf<IServiceProviderAwareModule>(
            x => x.Configure(applicationServices),
            appInitializationContext.Logger
        );
    }

    /// <summary>
    /// Adds all modules to DI Container as interfaces they implement.
    /// Also registers <see cref="AppInitializationContext"/>.
    /// </summary>
    /// <param name="services">DI Container.</param>
    /// <param name="modules">List of modules to be used.</param>
    /// <param name="appInitializationContext">Application initialization context.</param>
    protected virtual void RegisterModulesAsServices(
        [NotNull] IServiceCollection services,
        [NotNull, ItemNotNull] IReadOnlyCollection<IModule> modules,
        [NotNull] AppInitializationContext appInitializationContext
    )
    {
        var moduleType = typeof(IModule);
        services.AddSingleton(appInitializationContext);

        foreach (var module in modules)
        {
            var typeOfModule = module.GetType();
            var selfServiceDescriptor = ServiceDescriptor.Singleton(typeOfModule, module);
            services.Add(selfServiceDescriptor);

            var moduleServiceDescriptor = ServiceDescriptor.Singleton(moduleType, sp => sp.GetService(typeOfModule));
            services.Add(moduleServiceDescriptor);

            var implementedInterfaces = typeOfModule
                                        .GetInterfaces()
                                        .Where(x => x != moduleType);
            foreach (var implementedInterface in implementedInterfaces)
            {
                var descriptor = ServiceDescriptor.Singleton(implementedInterface, sp => sp.GetService(typeOfModule));
                services.Add(descriptor);
            }
        }
    }

    private void AddHealthChecksInternal(
        IServiceCollection services,
        AppInitializationContext context,
        IEnumerable<IModule> allModules
    )
    {
        var healthChecksBuilder = services.AddHealthChecks();
        healthChecksBuilder.AddCheck(
            HealthCheckDefinitions.SelfHealthCheckPrefix + "check",
            () => HealthCheckResult.Healthy()
        );
        healthChecksBuilder.AddCheck<ServiceInitializedHealthCheck>(
            HealthCheckDefinitions.StartupHealthCheckPrefix + "check"
        );

        var modules = allModules.OfType<IHealthCheckAwareModule>().ToArray();
        if (modules.Length == 0)
        {
            if (!SuppressNoHealthCheckWarning)
            {
                context.Logger.LogWarning(
                    "No health-check related modules was found. There will be only 'self'-check added to list of health-checks"
                    + "(it is used for liveness probe, returns healthy in case request for health can be served at all)."
                );
            }

            return;
        }

        modules.ForEachOf<IHealthCheckAwareModule>(
            x => x.Configure(healthChecksBuilder),
            context.Logger
        );
    }

    private static void RegisterModuleServicesInternal(
        IServiceCollection services,
        AppInitializationContext context,
        IEnumerable<IModule> modules
    )
    {
        modules.ForEachOf<IServiceCollectionAwareModule>(
            x => x.Configure(services, context.Configuration),
            context.Logger
        );

        var builder = new HandlerMetadataStore.Builder(services);
        services.TryAddSingleton<IHandlerMetadataStore>(builder.Build);
    }

    private static void ValidateModules(IEnumerable<IModule> modules, ILogger appLogger)
    {
        var duplicateModules = modules.GroupBy(x => x.ModuleIdentity)
                                      .Where(
                                          x => x.Count() > 1
                                      ).ToArray();
        if (duplicateModules.Length > 0)
        {
            var ex = new DuplicateModulesException(duplicateModules);
            appLogger.LogCritical(ex, "Problem during application initialization was found. Details: {details}", ex.Message);
            throw ex;
        }
    }

    private void AddTelemetryInternal(
        IServiceCollection services,
        AppInitializationContext context,
        IReadOnlyCollection<IModule> allModules
    )
    {
        var invoker = GetInvoker();
        invoker.Configure(services, context, allModules);
    }
}
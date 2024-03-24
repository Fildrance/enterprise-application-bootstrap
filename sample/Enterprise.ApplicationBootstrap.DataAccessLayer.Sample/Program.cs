using System.Collections.Generic;
using System.Threading.Tasks;
using Enterprise.ApplicationBootstrap.Core;
using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using Enterprise.ApplicationBootstrap.Core.Topology;
using Enterprise.ApplicationBootstrap.DataAccessLayer.PgSql;
using Enterprise.ApplicationBootstrap.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Sample;

internal class Program : ServiceProgramBase
{
    static Task Main(string[] args)
    {
        var defaultBuilder = Host.CreateDefaultBuilder()
                                 .UseEnvironment("Development");
        return new Program().BuildApplication(defaultBuilder, args)
                            .RunAsync();
    }

    /// <inheritdoc />
    protected override IReadOnlyCollection<IModule> GetModules(AppInitializationContext context)
    {
        return new IModule[]
        {
            new SampleDataAccessLayerModule("sample", context)
        };
    }

    /// <inheritdoc />
    protected override IConfigurationBuilder SetupConfiguration(IConfigurationBuilder configurationManager, IHostEnvironment hostEnvironment, string[] commandLineArgs)
    {
        configurationManager.AddJsonFile("appsettings.json");
        configurationManager.AddJsonFile($"appsettings.{hostEnvironment.EnvironmentName}.json", optional: true);
        return base.SetupConfiguration(configurationManager, hostEnvironment, commandLineArgs);
    }

    /// <inheritdoc />
    protected override TelemetrySetupInvokerBase GetInvoker()
        => new TelemetrySetupInvoker();
}

public class SampleDataAccessLayerModule(string connectionStringName, AppInitializationContext context)
    : NpgsqlDataAccessLayerModuleBase(connectionStringName, context)
{
}
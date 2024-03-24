using System.Collections.Generic;
using System.Threading.Tasks;
using Enterprise.ApplicationBootstrap.Core;
using Enterprise.ApplicationBootstrap.Core.Api.Modules;
using Enterprise.ApplicationBootstrap.Core.Topology;
using Enterprise.ApplicationBootstrap.Telemetry;
using Enterprise.ApplicationBootstrap.WebApi.OpenAPi;
using Enterprise.ApplicationBootstrap.WebApi.Sample.Modules;
using Microsoft.AspNetCore.Builder;

namespace Enterprise.ApplicationBootstrap.WebApi.Sample;

public class Program : WebApiServiceProgramBase
{
    static Task Main(string[] args)
        => new Program()
           .BuildApplication(
               WebApplication.CreateBuilder(),
               args
           ).RunAsync();

    /// <inheritdoc />
    protected override TelemetrySetupInvokerBase GetInvoker()
        => new TelemetrySetupInvoker();

    /// <inheritdoc />
    protected override IReadOnlyCollection<IModule> GetWebApiModules(AppInitializationContext context)
        => new IModule[]
        {
            new DefaultOpenTelemetryTracingModule(context),
            new SampleModule(),
            new DefaultOpenApiModule(context),
            new SampleEndpointsConfigModule(),
            //new DefaultApiVersionModule()
        };
}
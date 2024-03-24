using System.Collections.Generic;
using Enterprise.ApplicationBootstrap.WebApi.Modules;
using Enterprise.ApplicationBootstrap.WebApi.Modules.Endpoints;
using Microsoft.AspNetCore.Routing;

namespace Enterprise.ApplicationBootstrap.WebApi.Sample.Modules;

internal class SampleEndpointsConfigModule : EndpointRouteConfigureAwareModuleBase
{
    /// <inheritdoc />
    public override string ModuleIdentity => "SampleEndpointsConfigModule";


    /// <inheritdoc />
    public override IEnumerable<IEndpointRouteBuilderAggregator> GetEndpointRouteBuilders(IEndpointRouteBuilder endpointRouteBuilder)
    {
        //var versionSet = endpointRouteBuilder.NewApiVersionSet()
        //                                     .ReportApiVersions()
        //                                     .HasApiVersion(1.0)
        //                                     .HasApiVersion(2.0)
        //                                     .Build();

        yield return CreateAggregator<SampleService>(builder =>
        {
            return new[]
            {
                builder.MapPost<SampleRequest2>("/api/for-dev")
                       .To((service, request, ct) => service.Process2(request, ct)),
                //.MapToApiVersion(1.0),
                builder.MapGet<SampleRequest>("/api/for-dev")
                .To((service, request, ct) => service.Process(request, ct))
                //.MapToApiVersion(2.0),
            };
        }); //.Predefine((x, _) => x.WithApiVersionSet(versionSet));
    }
}
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Enterprise.ApplicationBootstrap.WebApi.Sample;

public class SampleService(IMediator mediator)
{
    public Task<SampleResponse> Process(SampleRequest request, CancellationToken ct)
        => mediator.Send(request, ct);

    public Task Process2(SampleRequest2 request, CancellationToken ct)
        => mediator.Send(request, ct);
}

public class SampleRequest2 : IRequest
{
}

public class Sample2Handler : IRequestHandler<SampleRequest2>
{
    /// <inheritdoc />
    public Task Handle(SampleRequest2 request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
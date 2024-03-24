using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Enterprise.ApplicationBootstrap.WebApi.Sample;

public class SampleRequest : IRequest<SampleResponse>
{
    public int? Count { get; set; }
    public string Chu { get; set; }
    public State State { get; set; }
}

public enum State
{
    First,
    Second
}

public class SampleResponse
{
    public string Text { get; set; }
}

public class SampleRequestHandler : IRequestHandler<SampleRequest, SampleResponse>
{
    /// <inheritdoc />
    public Task<SampleResponse> Handle(SampleRequest request, CancellationToken cancellationToken)
    {
        //throw new InvalidOperationException("wee fuk");
        return Task.FromResult(new SampleResponse { Text = "you bloody bastard" + request.Count });
    }
}
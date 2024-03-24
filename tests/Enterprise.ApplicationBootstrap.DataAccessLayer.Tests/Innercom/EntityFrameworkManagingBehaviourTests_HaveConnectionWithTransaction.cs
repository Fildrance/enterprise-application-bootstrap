using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ApplicationBootstrap.Core.HandlerMetadata;
using Enterprise.ApplicationBootstrap.DataAccessLayer.Attributes;
using Enterprise.ApplicationBootstrap.DataAccessLayer.Repository;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using IsolationLevel = System.Data.IsolationLevel;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Tests.Innercom;

[Xunit.Trait("Category", "Innercom"), ExcludeFromCodeCoverage]
public class EntityFrameworkManagingBehaviourTests_HaveConnectionWithTransaction : EntityFrameworkManagingBehaviourTestsBase
{
    private readonly TestRequest1 _testRequest = new();

    public EntityFrameworkManagingBehaviourTests_HaveConnectionWithTransaction()
    {
        _metadataStore.Get<TestRequest1, TestResponse1>()
                      .Returns(new HandlerMetadataInfo(typeof(Handler1)));
        _metadataStore.Get<TestRequest2, TestResponse2>()
                      .Returns(new HandlerMetadataInfo(typeof(Handler11)));
        _metadataStore.Get<TestRequest3, TestResponse3>()
                      .Returns(new HandlerMetadataInfo(typeof(Handler12)));
    }

    /// <inheritdoc />
    protected override void RegisterHandlers(ServiceCollection sc)
    {
        sc.AddSingleton<IRequestHandler<TestRequest1, TestResponse1>, Handler1>();
        sc.AddSingleton<IRequestHandler<TestRequest2, TestResponse2>, Handler11>();
        sc.AddSingleton<IRequestHandler<TestRequest3, TestResponse3>, Handler12>();
    }

    [Fact]
    public async Task HaveConnectionOnTopLevel_WithTransaction()
    {
        //arrange
        var dbConnection = Substitute.For<DbConnection>();
        var dbTransaction = Substitute.For<DbTransaction>();
        dbConnection.BeginTransactionAsync(IsolationLevel.ReadCommitted, Arg.Any<CancellationToken>())
                    .Returns(dbTransaction);

        _connectionFactory.CreateAndOpenConnectionAsync(Arg.Any<CancellationToken>())
                          .Returns(dbConnection);

        _dbContextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
                         .Returns(Task.FromResult(_stubDbContext));

        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        //act
        _ = await _mediator.Send(_testRequest, ct);

        //assert
        Received.InOrder(() =>
        {
            _dbContextHelper.SetConnection(_stubDbContext, dbConnection);
            _dbContextHelper.SetTransactionAsync(_stubDbContext, dbTransaction, ct);

            _set.AddAsync(Arg.Is<TestEntity>(e => e.Value == 1), ct);
            _stubDbContext.SaveChangesAsync(ct);
            _set.AddAsync(Arg.Is<TestEntity>(e => e.Value == 2), ct);
            _stubDbContext.SaveChangesAsync(ct);
            dbConnection.DisposeAsync();
        });

        _dbContextHelper.Received(1).SetConnection(_stubDbContext, dbConnection);

        dbConnection.DidNotReceiveWithAnyArgs().BeginTransaction();
    }

    #region types for test

    [RequireDbContext(IsolationLevel.ReadCommitted)]
    private class Handler1(IMediator mediator) : IRequestHandler<TestRequest1, TestResponse1>
    {
        /// <inheritdoc />
        public async Task<TestResponse1> Handle(TestRequest1 request, CancellationToken cancellationToken)
        {
            await mediator.Send(new TestRequest2(), cancellationToken);
            await mediator.Send(new TestRequest3(), cancellationToken);
            return new TestResponse1();
        }
    }

    private class Handler11(IRepository<TestEntity, int> repo) : IRequestHandler<TestRequest2, TestResponse2>
    {
        private readonly IRepository<TestEntity, int> _repo = repo;

        /// <inheritdoc />
        public async Task<TestResponse2> Handle(TestRequest2 request, CancellationToken cancellationToken)
        {
            await _repo.Add(new TestEntity { Value = 1 }, cancellationToken);
            return new();
        }
    }

    private class Handler12(IRepository<TestEntity, int> repo) : IRequestHandler<TestRequest3, TestResponse3>
    {
        /// <inheritdoc />
        public async Task<TestResponse3> Handle(TestRequest3 request, CancellationToken cancellationToken)
        {
            await repo.Add(new TestEntity { Value = 2 }, cancellationToken);
            return new();
        }
    }

    private class TestRequest1 : IRequest<TestResponse1>;

    private class TestResponse1;


    private class TestRequest2 : IRequest<TestResponse2>;

    private class TestResponse2;


    private class TestRequest3 : IRequest<TestResponse3>;

    private class TestResponse3;

    #endregion
}
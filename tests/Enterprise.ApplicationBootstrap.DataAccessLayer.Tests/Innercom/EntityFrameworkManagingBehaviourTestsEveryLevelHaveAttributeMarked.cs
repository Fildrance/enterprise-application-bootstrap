using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ApplicationBootstrap.Core.HandlerMetadata;
using Enterprise.ApplicationBootstrap.DataAccessLayer.Attributes;
using Enterprise.ApplicationBootstrap.DataAccessLayer.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Tests.Innercom;

[Xunit.Trait("Category", "Innercom"), ExcludeFromCodeCoverage]
public class EntityFrameworkManagingBehaviourTestsEveryLevelHaveAttributeMarked : EntityFrameworkManagingBehaviourTestsBase
{
    private readonly TestRequest1 _testRequest = new();

    public EntityFrameworkManagingBehaviourTestsEveryLevelHaveAttributeMarked()
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
        sc.AddSingleton<IRequestHandler<TestRequest2, TestResponse2>>(sp =>
        {
            var l = new Handler11(sp.GetRequiredService<IRepository<TestEntity, int>>());
            return l;
        });
        sc.AddSingleton<IRequestHandler<TestRequest3, TestResponse3>, Handler12>();
    }

    [Fact]
    public async Task HaveNoAttributeOnHighLevel()
    {
        //arrange
        _dbContextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
                         .Returns(_stubDbContext);

        var dbConnection1 = Substitute.For<DbConnection>();
        _connectionFactory.CreateAndOpenConnectionAsync(Arg.Any<CancellationToken>())
                          .Returns(dbConnection1);

        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        //act
        _ = await _mediator.Send(_testRequest, ct);

        //assert
        Received.InOrder(() =>
        {
            _dbContextHelper.SetConnection(_stubDbContext, dbConnection1);
            _set.AddAsync(Arg.Is<TestEntity>(e => e.Value == 1), ct);
            _stubDbContext.SaveChangesAsync(ct);
            _set.AddAsync(Arg.Is<TestEntity>(e => e.Value == 2), ct);
            _stubDbContext.SaveChangesAsync(ct);
            dbConnection1.DisposeAsync();
            _stubDbContext.DisposeAsync();
        });
        _connectionFactory.Received(1).CreateAndOpenConnectionAsync(Arg.Any<CancellationToken>());
        _dbContextHelper.Received(1).SetConnection(Arg.Any<DbContext>(), Arg.Any<DbConnection>());

        _dbContextFactory.Received(1).CreateDbContextAsync(Arg.Any<CancellationToken>());
        _connectionFactory.DidNotReceive().CreateAndOpenConnection();
        _dbContextHelper.DidNotReceive().SetTransaction(Arg.Any<DbContext>(), Arg.Any<DbTransaction>());
        _dbContextHelper.DidNotReceive().SetTransactionAsync(Arg.Any<DbContext>(), Arg.Any<DbTransaction>(), Arg.Any<CancellationToken>());
    }

    #region types for test

    [RequireDbContext]
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

    [RequireDbContext]
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
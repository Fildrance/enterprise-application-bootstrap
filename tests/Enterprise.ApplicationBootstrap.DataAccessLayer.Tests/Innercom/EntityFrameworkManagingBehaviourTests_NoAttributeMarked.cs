using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ApplicationBootstrap.Core.HandlerMetadata;
using Enterprise.ApplicationBootstrap.DataAccessLayer.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

// ReSharper disable MethodHasAsyncOverloadWithCancellation

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Tests.Innercom;

[Xunit.Trait("Category", "Innercom"), ExcludeFromCodeCoverage]
public class EntityFrameworkManagingBehaviourTests_NoAttributeMarked : EntityFrameworkManagingBehaviourTestsBase
{
    private readonly TestRequest1 _testRequest = new();

    public EntityFrameworkManagingBehaviourTests_NoAttributeMarked()
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
        var dbContext1 = CreateDbContextStub();
        var set1 = Substitute.For<DbSet<TestEntity>>();
        dbContext1.Set<TestEntity>()
                  .Returns(set1);
        var dbContext2 = CreateDbContextStub();
        var set2 = Substitute.For<DbSet<TestEntity>>();
        dbContext2.Set<TestEntity>()
                  .Returns(set2);
        _dbContextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
                         .Returns(_ => dbContext1, _ => dbContext2);

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
            _dbContextHelper.SetConnection(dbContext1, dbConnection1);
            set1.AddAsync(Arg.Is<TestEntity>(e => e.Value == 1), ct);
            dbContext1.SaveChangesAsync(ct);
            dbContext1.DisposeAsync();
            _dbContextHelper.SetConnection(dbContext2, dbConnection1);
            set2.AddAsync(Arg.Is<TestEntity>(e => e.Value == 2), ct);
            dbContext2.SaveChangesAsync(ct);
            dbContext2.DisposeAsync();
            dbConnection1.DisposeAsync();
        });
        _connectionFactory.Received(1).CreateAndOpenConnectionAsync(Arg.Any<CancellationToken>());
        _dbContextHelper.Received(2).SetConnection(Arg.Any<DbContext>(), Arg.Any<DbConnection>());

        _connectionFactory.DidNotReceive().CreateAndOpenConnection();
        _dbContextHelper.DidNotReceive().SetTransaction(Arg.Any<DbContext>(), Arg.Any<DbTransaction>());
        _dbContextHelper.DidNotReceive().SetTransactionAsync(Arg.Any<DbContext>(), Arg.Any<DbTransaction>(), Arg.Any<CancellationToken>());
    }

    #region types for test

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
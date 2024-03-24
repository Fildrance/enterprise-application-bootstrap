using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Enterprise.ApplicationBootstrap.Core;
using Enterprise.ApplicationBootstrap.Core.Api;
using Enterprise.ApplicationBootstrap.Core.HandlerMetadata;
using Enterprise.ApplicationBootstrap.DataAccessLayer.Configuration;
using Enterprise.ApplicationBootstrap.DataAccessLayer.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using NSubstitute;
using Xunit.Sdk;

// ReSharper disable InconsistentNaming

namespace Enterprise.ApplicationBootstrap.DataAccessLayer.Tests.Innercom;

[ExcludeFromCodeCoverage]
public abstract class EntityFrameworkManagingBehaviourTestsBase
{
    protected readonly IHandlerMetadataStore _metadataStore = Substitute.For<IHandlerMetadataStore>();

    protected readonly CustomDbContext _stubDbContext = CreateDbContextStub();
    
    protected readonly IConnectionFactory _connectionFactory = Substitute.For<IConnectionFactory>();
    protected readonly IDbContextFactory<CustomDbContext> _dbContextFactory = Substitute.For<IDbContextFactory<CustomDbContext>>();
    protected readonly IDbContextHelper _dbContextHelper = Substitute.For<IDbContextHelper>();
    protected readonly DbSet<TestEntity> _set = Substitute.For<DbSet<TestEntity>>();
    protected readonly IMediator _mediator;

    protected EntityFrameworkManagingBehaviourTestsBase()
    {
        var sc = new ServiceCollection();
        sc.AddLogging();
        sc.AddSingleton<ObjectPool<Dictionary<string, object>>>(
            new DefaultObjectPool<Dictionary<string, object>>(new CollectionCleaningPoolPolicy()));
        sc.AddSingleton<IMediator, Mediator>();
        sc.AddSingleton(typeof(IPipelineBehavior<,>), typeof(SessionInitializingBehaviour<,>));
        sc.AddSingleton(typeof(IPipelineBehavior<,>), typeof(EntityFrameworkManagingBehaviour<,>));

        sc.AddSingleton<IHandlerMetadataStore>(sp => new HandlerMetadataStore.Builder(sc).Build(sp));
        sc.AddSingleton<IDbContextManager, DbContextManager>();
        sc.AddSingleton<IConnectionFactory>(_connectionFactory);
        sc.AddSingleton<IDbContextFactory<CustomDbContext>>(_dbContextFactory);
        sc.AddSingleton<IDbContextHelper>(_dbContextHelper);
        _stubDbContext.Set<TestEntity>().Returns(_set);

        sc.AddSingleton<IRepository<TestEntity, int>, RepositoryForTests>();

        RegisterHandlers(sc);
        var sp = sc.BuildServiceProvider();
        _mediator = sp.GetRequiredService<IMediator>();
    }

    protected abstract void RegisterHandlers(ServiceCollection sc);

    protected static bool IsNestedIntoLevel(Session session, int i)
    {
        int current = 0;
        while (current < i)
        {
            session = NestedExtractor(session);
            if (session == null)
            {
                throw new AssertActualExpectedException(
                    $"Session was nested {i} levels deep",
                    $"On {current} level ther is already root session",
                    "Failed check for nesting level of session"
                );
            }

            current++;
        }

        return true;

        Session NestedExtractor(Session s) => (Session)typeof(Session).GetField("_nested", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(s);
    }

    protected class RepositoryForTests(IDbContextManager dbContextManager) : RepositoryBase<TestEntity, int>(dbContextManager);

    public class TestEntity : EntityBase<int>
    {
        public int Value { get; set; }
    }

    protected static CustomDbContext CreateDbContextStub()
    {
        return Substitute.For<CustomDbContext>(
            new DbContextOptions<CustomDbContext>(),
            new List<IContextConfiguration>()
        );
    }
}
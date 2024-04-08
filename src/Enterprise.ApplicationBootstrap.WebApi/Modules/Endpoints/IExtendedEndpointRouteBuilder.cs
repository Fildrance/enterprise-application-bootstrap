using System;
using System.Linq.Expressions;
using System.Threading;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Enterprise.ApplicationBootstrap.WebApi.Modules.Endpoints;

/// <summary>
/// Intermediate builder for setting up route handler. Works as 2-part call with <see cref="IExtendedEndpointRouteBuilder{TService,TRequest}"/>.
/// </summary>
/// <typeparam name="TService">Type of <see cref="IMediator"/>-derived service.</typeparam>
public interface IExtendedEndpointRouteBuilder<TService> : IEndpointRouteBuilder
{
    /// <summary>
    /// Sets route and GET http-method for route handler to be configured and returns intermediate builder that can bind route with service call.
    /// </summary>
    /// <typeparam name="TRequest">Type of contract that will be passed from web-request to service call.</typeparam>
    /// <param name="route">Route for which handler will be configured.</param>
    /// <returns>Builder that will require binding <see cref="route"/> to some <see cref="IMediator"/>-based service call</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="route"/> is null.</exception>
    [NotNull]
    IExtendedEndpointRouteBuilder<TService, TRequest> MapGet<TRequest>([NotNull] string route) where TRequest : IBaseRequest;

    /// <summary>
    /// Sets route and POST http-method for route handler to be configured and returns intermediate builder that can bind route with service call.
    /// </summary>
    /// <typeparam name="TRequest">Type of contract that will be passed from web-request to service call.</typeparam>
    /// <param name="route">Route for which handler will be configured.</param>
    /// <returns>Builder that will require binding <see cref="route"/> to some <see cref="IMediator"/>-based service call</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="route"/> is null.</exception>
    [NotNull]
    IExtendedEndpointRouteBuilder<TService, TRequest> MapPost<TRequest>([NotNull] string route) where TRequest : IBaseRequest;

    /// <summary>
    /// Sets route and DELETE http-method for route handler to be configured and returns intermediate builder that can bind route with service call.
    /// </summary>
    /// <typeparam name="TRequest">Type of contract that will be passed from web-request to service call.</typeparam>
    /// <param name="route">Route for which handler will be configured.</param>
    /// <returns>Builder that will require binding <see cref="route"/> to some <see cref="IMediator"/>-based service call</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="route"/> is null.</exception>
    [NotNull]
    IExtendedEndpointRouteBuilder<TService, TRequest> MapDelete<TRequest>([NotNull] string route) where TRequest : IBaseRequest;

    /// <summary>
    /// Sets route and PUT http-method for route handler to be configured and returns intermediate builder that can bind route with service call.
    /// </summary>
    /// <typeparam name="TRequest">Type of contract that will be passed from web-request to service call.</typeparam>
    /// <param name="route">Route for which handler will be configured.</param>
    /// <returns>Builder that will require binding <see cref="route"/> to some <see cref="IMediator"/>-based service call</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="route"/> is null.</exception>
    [NotNull]
    IExtendedEndpointRouteBuilder<TService, TRequest> MapPut<TRequest>([NotNull] string route) where TRequest : IBaseRequest;

    /// <summary>
    /// Sets route and PATCH http-method for route handler to be configured and returns intermediate builder that can bind route with service call.
    /// </summary>
    /// <typeparam name="TRequest">Type of contract that will be passed from web-request to service call.</typeparam>
    /// <param name="route">Route for which handler will be configured.</param>
    /// <returns>Builder that will require binding <see cref="route"/> to some <see cref="IMediator"/>-based service call</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="route"/> is null.</exception>
    [NotNull]
    IExtendedEndpointRouteBuilder<TService, TRequest> MapPatch<TRequest>([NotNull] string route) where TRequest : IBaseRequest;
}

/// <summary>
/// Intermediate builder that helps to bind http-method and relative route to <see cref="IMediator"/>-based service-call.
/// </summary>
/// <typeparam name="TService">Type of <see cref="IMediator"/>-based service.</typeparam>
/// <typeparam name="TRequest">Type of contract which is expected to be passed to service call from http-request.</typeparam>
public interface IExtendedEndpointRouteBuilder<TService, TRequest> where TRequest : IBaseRequest
{
    /// <summary>
    /// Binds http-method and relative route to service call.
    /// </summary>
    /// <typeparam name="TResponse">Type of response which is expected to be returned from service call to http-response.</typeparam>
    /// <param name="handleExpression">
    /// Expression with service call, which will handler http-request and provide response data.
    /// Expression should be simple and should not contain any type of data convert, transformation or additional method calls.
    /// Should contain only single call to <see cref="TService"/> service, which will return result of request.
    /// </param>
    /// <returns>Prepared RouteHandlerBuilder.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="handleExpression"/> is null.</exception>
    /// <exception cref="InvalidCastException">When <paramref name="handleExpression"/> is not single simple call to <see cref="TService"/> service.</exception>
    [NotNull]
    RouteHandlerBuilder To<TResponse>(
        [NotNull] Expression<Func<TService, TRequest, CancellationToken, TResponse>> handleExpression
    );
}
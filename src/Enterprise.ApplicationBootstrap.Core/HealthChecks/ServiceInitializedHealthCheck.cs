using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ApplicationBootstrap.Core.Initialization;
using JetBrains.Annotations;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Enterprise.ApplicationBootstrap.Core.HealthChecks;

/// <summary>
/// Health-check that returns healthy-ness of application based on state of all <see cref="IOnApplicationStartInitializable"/> services.
/// Returns <see cref="HealthCheckResult.Healthy"/> if all services have <see cref="IOnApplicationStartInitializable.Initialized"/> = true,
/// and <see cref="HealthCheckResult.Unhealthy"/> otherwise.
/// </summary>
[PublicAPI]
public class ServiceInitializedHealthCheck : IHealthCheck
{
    private bool _isInitialized;
    private static readonly HealthCheckResult HealthyResult = HealthCheckResult.Healthy("Internal services are initialized!");

    private readonly IReadOnlyCollection<IOnApplicationStartInitializable> _initializableServices;

    /// <summary> c-tor. </summary>
    public ServiceInitializedHealthCheck(
        [NotNull, ItemNotNull] IEnumerable<IOnApplicationStartInitializable> initializableServices
    )
    {
        _initializableServices = initializableServices.ToArray();
    }

    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        if (_isInitialized)
        {
            return Task.FromResult(HealthyResult);
        }

        var nonInitialized = _initializableServices.Where(x => !x.Initialized)
                                                   .ToArray();
        if (nonInitialized.Length == 0)
        {
            _isInitialized = true;
            return Task.FromResult(HealthyResult);
        }

        var unhealthyResult = HealthCheckResult.Unhealthy(
            "Not all services were initialized yet! " 
            + $"List of non-initialized ones: {GetServicesNames(nonInitialized)}."
        );
        return Task.FromResult(unhealthyResult);
    }

    private static string GetServicesNames(IEnumerable<IOnApplicationStartInitializable> initializableServices)
    {
        var names = initializableServices
            .Select(x => x.GetType().FullName);
        return $"'{string.Join("', '", names)}'";
    }
}
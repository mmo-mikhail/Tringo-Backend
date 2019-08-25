using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tringo.FlightsService;

namespace Tringo.WebApp.HealthChecks
{
    public class MainHealthCheck : IHealthCheck
    {
        private readonly IFlightsService _flightsService;

        public MainHealthCheck(IFlightsService flightsService)
        {
            _flightsService = flightsService;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var healthCheckResultHealthy =
                _flightsService.GetAirports().Any()
                && _flightsService.GetFlights().Any();

            if (healthCheckResultHealthy)
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy("The check indicates a healthy result."));
            }

            return Task.FromResult(
                HealthCheckResult.Unhealthy("The check indicates an unhealthy result."));
        }
    }
}

using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using Xunit;

namespace ECommerce.Api.Tests.Integration;

public class ObservabilityTests
{
    [Fact]
    public void TracerProvider_ShouldResolve_WithoutCrashing()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddMassTransitTestHarness();
            });
        });

        using var scope = factory.Services.CreateScope();
        var tracerProvider = scope.ServiceProvider.GetService<TracerProvider>();

        Assert.NotNull(tracerProvider);
    }
}

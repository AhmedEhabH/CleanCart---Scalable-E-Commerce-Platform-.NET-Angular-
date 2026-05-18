using Hangfire;
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
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.AddHangfire(config => config.UseInMemoryStorage());
            });
        });

        using var scope = factory.Services.CreateScope();
        var tracerProvider = scope.ServiceProvider.GetService<TracerProvider>();

        Assert.NotNull(tracerProvider);
    }
}
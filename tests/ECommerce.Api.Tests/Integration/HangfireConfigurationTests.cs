using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using MassTransit;
using Xunit;

namespace ECommerce.Api.Tests.Integration;

public class HangfireConfigurationTests
{
    [Fact]
    public async Task HangfireDashboard_ShouldReturn401_ForUnauthenticatedRequest()
    {
        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddHangfire(config => config.UseInMemoryStorage());
                services.AddMassTransitTestHarness();
            });
        });
        var client = factory.CreateClient();

        var response = await client.GetAsync("/hangfire");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

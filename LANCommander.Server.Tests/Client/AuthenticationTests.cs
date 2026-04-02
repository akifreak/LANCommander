using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using LANCommander.SDK.Services;

namespace LANCommander.Server.Tests.Client;

[Collection("Application")]
public class AuthenticationTests : IClassFixture<ApplicationFixture>
{
    private readonly ApplicationFixture _fixture;

    public AuthenticationTests(ApplicationFixture fixture)
    {
        _fixture = ApplicationFixture.Instance;
    }

    [Fact]
    public async Task PingShouldWork()
    {
        var authClient = _fixture.ServiceProvider.GetRequiredService<AuthenticationClient>();
        var response = await authClient.ValidateTokenAsync();
        
        response.ShouldBeTrue();
    }
}
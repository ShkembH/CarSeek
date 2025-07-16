using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CarSeek.IntegrationTests;

[Collection("Integration Tests")]
public abstract class TestBase : IDisposable
{
    protected readonly HttpClient _client;
    protected readonly CustomWebApplicationFactory _factory;

    public TestBase(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _factory.ResetDatabase();

        // Clear any existing authorization headers
        _client.DefaultRequestHeaders.Authorization = null;
    }

    public virtual void Dispose()
    {
        _client?.Dispose();
        // Reset database after test to ensure clean state
        _factory.ResetDatabase();
    }
}

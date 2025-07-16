using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using CarSeek.Application.Features.Auth.Common;
using CarSeek.Domain.Enums;
using Xunit;

namespace CarSeek.IntegrationTests.Features.Auth;

public class AuthenticationTests : TestBase
{
    public AuthenticationTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var request = new
        {
            Email = "test@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.Individual // Changed from UserRole.Regular to UserRole.Individual
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotBeNull();
        content!.Email.Should().Be(request.Email);
        content.FirstName.Should().Be(request.FirstName);
        content.LastName.Should().Be(request.LastName);
        content.Role.Should().Be(request.Role);
        content.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new
        {
            Email = "existing@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.Individual // Changed from UserRole.Regular to UserRole.Individual
        };

        // Register first time
        await _client.PostAsJsonAsync("/api/auth/register", request);

        // Act - Register second time with same email
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var registerRequest = new
        {
            Email = "login@example.com",
            Password = "Test123!",
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.Individual // Changed from UserRole.Regular to UserRole.Individual
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new
        {
            Email = "login@example.com",
            Password = "Test123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var content = await response.Content.ReadFromJsonAsync<AuthResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotBeNull();
        content!.Email.Should().Be(loginRequest.Email);
        content.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new
        {
            Email = "invalid@example.com",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

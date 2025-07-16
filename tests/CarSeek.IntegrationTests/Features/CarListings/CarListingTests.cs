using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using CarSeek.Application.Features.CarListings.DTOs;
using CarSeek.Domain.Enums;
using CarSeek.Application.Features.Auth.Common;
using CarSeek.Application.Features.Dealerships.DTOs;
using Xunit;

namespace CarSeek.IntegrationTests.Features.CarListings;

public class CarListingTests : TestBase
{
    public CarListingTests(CustomWebApplicationFactory factory) : base(factory) { }

    // Remove the custom Dispose method - let the base class handle it

    private async Task<AuthResponse> SetupDealershipUserAndGetToken()
    {
        var registerRequest = new RegisterRequest(
            Email: "dealer@test.com",
            Password: "Test123!",
            FirstName: "Test",
            LastName: "Dealer",
            PhoneNumber: null,
            Country: null,
            City: null,
            Role: UserRole.Dealership,
            CompanyName: null,
            CompanyUniqueNumber: null,
            Location: null,
            BusinessCertificatePath: null);

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "dealer@test.com",
            Password = "Test123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var result = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        return result ?? throw new InvalidOperationException("Login failed");
    }

    private async Task<DealershipDto> CreateDealership(AuthResponse token)
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);

        var createRequest = new CreateDealershipRequest
        {
            Name = "Test Dealership",
            Description = "Test Description",
            Street = "123 Test St",
            City = "Test City",
            State = "Test State",
            PostalCode = "12345",
            Country = "Test Country",
            PhoneNumber = "123-456-7890",
            Website = "https://testdealership.com"
        };

        var response = await _client.PostAsJsonAsync("/api/dealerships", createRequest);
        var result = await response.Content.ReadFromJsonAsync<DealershipDto>();
        return result ?? throw new InvalidOperationException("Failed to create dealership");
    }

    [Fact]
    public async Task UpdateStatus_WithValidTransition_ReturnsUpdatedListing()
    {
        // Arrange
        var token = await SetupDealershipUserAndGetToken();
        await CreateDealership(token);

        // IMPORTANT: Set the token for all subsequent requests
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);

        var createRequest = new CreateCarListingRequest
        {
            Title = "2023 Tesla Model Y",
            Description = "Electric SUV",
            Year = 2023,
            Make = "Tesla",
            Model = "Model Y",
            Price = 55000,
            Mileage = 1000
        };

        var createResponse = await _client.PostAsJsonAsync("/api/carlistings", createRequest);
        var createdListing = await createResponse.Content.ReadFromJsonAsync<CarListingDto>();

        var updateStatusRequest = new UpdateListingStatusRequest
        {
            Status = ListingStatus.Active
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/carlistings/{createdListing!.Id}/status", updateStatusRequest);
        var result = await response.Content.ReadFromJsonAsync<CarListingDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Status.Should().Be(ListingStatus.Active);
    }

    [Fact]
    public async Task UpdateStatus_WithInvalidTransition_ReturnsBadRequest()
    {
        // Arrange
        var authResponse = await SetupDealershipUserAndGetToken();
        await CreateDealership(authResponse);

        // IMPORTANT: Ensure the token is set for ALL subsequent requests
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse.Token);

        var createRequest = new CreateCarListingRequest
        {
            Title = "2022 BMW M4",
            Description = "Sports car",
            Year = 2022,
            Make = "BMW",
            Model = "M4",
            Price = 75000,
            Mileage = 5000
        };

        var createResponse = await _client.PostAsJsonAsync("/api/carlistings", createRequest);
        var createdListing = await createResponse.Content.ReadFromJsonAsync<CarListingDto>();

        // First update to Sold status
        var soldStatusRequest = new UpdateListingStatusRequest
        {
            Status = ListingStatus.Sold
        };

        var soldResponse = await _client.PatchAsJsonAsync($"/api/carlistings/{createdListing!.Id}/status", soldStatusRequest);
        soldResponse.EnsureSuccessStatusCode();

        // Try to update from Sold to Active (invalid transition)
        var invalidStatusRequest = new UpdateListingStatusRequest
        {
            Status = ListingStatus.Active
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/carlistings/{createdListing.Id}/status", invalidStatusRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Simply check that the response contains error information
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("errors");
        responseContent.Should().Contain("Status");
        responseContent.Should().Contain("Cannot change status from Sold to Active");
    }

    [Fact]
    public async Task UpdateStatus_WithNonDealerUser_ReturnsForbidden()
    {
        // Arrange
        // First create a listing with dealer user
        var dealerToken = await SetupDealershipUserAndGetToken();
        await CreateDealership(dealerToken);

        var createRequest = new CreateCarListingRequest
        {
            Title = "2021 Audi RS7",
            Description = "Performance sedan",
            Year = 2021,
            Make = "Audi",
            Model = "RS7",
            Price = 85000,
            Mileage = 8000
        };

        var createResponse = await _client.PostAsJsonAsync("/api/carlistings", createRequest);
        var createdListing = await createResponse.Content.ReadFromJsonAsync<CarListingDto>();

        // Register and login as regular user
        // Register and login as regular user
        var regularUserRegister = new RegisterRequest(
            Email: "user@test.com",
            Password: "Test123!",
            FirstName: "Regular",
            LastName: "User",
            PhoneNumber: null,
            Country: null,
            City: null,
            Role: UserRole.Individual,
            CompanyName: null,
            CompanyUniqueNumber: null,
            Location: null,
            BusinessCertificatePath: null);

        await _client.PostAsJsonAsync("/api/auth/register", regularUserRegister);

        var regularUserLogin = new LoginRequest
        {
            Email = "user@test.com",
            Password = "Test123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", regularUserLogin);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Set regular user's token
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);

        var updateStatusRequest = new UpdateListingStatusRequest
        {
            Status = ListingStatus.Active
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/carlistings/{createdListing!.Id}/status", updateStatusRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateStatus_WithDifferentDealer_ReturnsForbidden()
    {
        // Arrange
        // First dealer creates a listing
        var firstDealerToken = await SetupDealershipUserAndGetToken();
        await CreateDealership(firstDealerToken);

        var createRequest = new CreateCarListingRequest
        {
            Title = "2020 Porsche 911",
            Description = "Sports car",
            Year = 2020,
            Make = "Porsche",
            Model = "911",
            Price = 95000,
            Mileage = 12000
        };

        var createResponse = await _client.PostAsJsonAsync("/api/carlistings", createRequest);
        var createdListing = await createResponse.Content.ReadFromJsonAsync<CarListingDto>();

        // Create second dealer
        // Create second dealer
        var dealer2Register = new RegisterRequest(
            Email: "dealer2@test.com",
            Password: "Test123!",
            FirstName: "Second",
            LastName: "Dealer",
            PhoneNumber: null,
            Country: null,
            City: null,
            Role: UserRole.Dealership,
            CompanyName: null,
            CompanyUniqueNumber: null,
            Location: null,
            BusinessCertificatePath: null);

        await _client.PostAsJsonAsync("/api/auth/register", dealer2Register);

        var dealer2Login = new LoginRequest
        {
            Email = "dealer2@test.com",
            Password = "Test123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", dealer2Login);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Set second dealer's token
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);

        var updateStatusRequest = new UpdateListingStatusRequest
        {
            Status = ListingStatus.Active
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/carlistings/{createdListing!.Id}/status", updateStatusRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
} // End of class

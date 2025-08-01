using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using CarSeek.Application.Features.Auth.Common;
using CarSeek.Application.Features.Dealerships.DTOs;
using CarSeek.Application.Features.CarListings.DTOs;
using CarSeek.Domain.Enums;
using Xunit;

namespace CarSeek.IntegrationTests.Features.Dealerships;

public class DealershipCommandsTests : TestBase
{
    public DealershipCommandsTests(CustomWebApplicationFactory factory) : base(factory) { }

    private async Task<AuthResponse> SetupDealershipUserAndGetToken()
    {
        var registerRequest = new RegisterRequest(
            Email: "dealer@test.com",
            Password: "Dealer123!",
            FirstName: "Test",
            LastName: "Dealer",
            PhoneNumber: "123-456-7890",
            Country: "Kosovo",
            City: "Pristina",
            Role: UserRole.Dealership,
            CompanyName: "Test Dealership",
            CompanyUniqueNumber: "123456789",
            Location: "Test Location",
            BusinessCertificate: null);

        var authResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        authResponse.EnsureSuccessStatusCode();
        var authResult = await authResponse.Content.ReadFromJsonAsync<AuthResponse>();
        
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult!.Token);

        return authResult;
    }

    private async Task<AuthResponse> SetupIndividualUserAndGetToken()
    {
        var registerRequest = new RegisterRequest(
            Email: "individual@test.com",
            Password: "Individual123!",
            FirstName: "Test",
            LastName: "Individual",
            PhoneNumber: "123-456-7890",
            Country: "Kosovo",
            City: "Pristina",
            Role: UserRole.Individual,
            CompanyName: null,
            CompanyUniqueNumber: null,
            Location: null,
            BusinessCertificate: null);

        var authResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        authResponse.EnsureSuccessStatusCode();
        var authResult = await authResponse.Content.ReadFromJsonAsync<AuthResponse>();
        
        return authResult!;
    }

    private CreateDealershipRequest CreateValidDealershipRequest()
    {
        return new CreateDealershipRequest
        {
            Name = "Test Dealership",
            CompanyUniqueNumber = "123456789",
            Location = "Test Location",
            PhoneNumber = "123-456-7890",
            Website = "https://testdealership.com",
            Description = "A test dealership for integration tests",
            Street = "Test Street",
            City = "Pristina",
            State = "Kosovo",
            PostalCode = "10000",
            Country = "Kosovo"
        };
    }

    [Fact]
    public async Task CreateDealership_WithValidData_ReturnsSuccess()
    {
        // Arrange
        await SetupDealershipUserAndGetToken();
        var request = CreateValidDealershipRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/api/dealerships", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DealershipDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be(request.Name);
        result.CompanyUniqueNumber.Should().Be(request.CompanyUniqueNumber);
        result.Location.Should().Be(request.Location);
        result.PhoneNumber.Should().Be(request.PhoneNumber);
        result.Website.Should().Be(request.Website);
        result.Description.Should().Be(request.Description);
    }

    [Fact]
    public async Task CreateDealership_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        await SetupDealershipUserAndGetToken();
        var request = CreateValidDealershipRequest();
        request.Name = ""; // Invalid: empty company name

        // Act
        var response = await _client.PostAsJsonAsync("/api/dealerships", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateDealership_AsIndividualUser_ReturnsForbidden()
    {
        // Arrange
        var individualUser = await SetupIndividualUserAndGetToken();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", individualUser.Token);
        
        var request = CreateValidDealershipRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/api/dealerships", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateDealership_Unauthorized_ReturnsUnauthorized()
    {
        // Arrange
        var request = CreateValidDealershipRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/api/dealerships", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDealershipAnalytics_AsDealershipOwner_ReturnsAnalytics()
    {
        // Arrange
        var dealer = await SetupDealershipUserAndGetToken();
        
        // Create a dealership first
        var createRequest = CreateValidDealershipRequest();
        var createResponse = await _client.PostAsJsonAsync("/api/dealerships", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdDealership = await createResponse.Content.ReadFromJsonAsync<DealershipDto>();

        // Act
        var response = await _client.GetAsync($"/api/dealerships/{createdDealership!.Id}/analytics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DealershipAnalyticsDto>();
        result.Should().NotBeNull();
        result!.TotalListings.Should().BeGreaterThanOrEqualTo(0);
        result.ViewsThisMonth.Should().BeGreaterThanOrEqualTo(0);
        result.InquiriesThisMonth.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetDealershipAnalytics_AsNonOwner_ReturnsForbidden()
    {
        // Arrange
        var dealer1 = await SetupDealershipUserAndGetToken();
        
        // Create a dealership
        var createRequest = CreateValidDealershipRequest();
        var createResponse = await _client.PostAsJsonAsync("/api/dealerships", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdDealership = await createResponse.Content.ReadFromJsonAsync<DealershipDto>();

        // Switch to different user
        var dealer2 = await SetupIndividualUserAndGetToken();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", dealer2.Token);

        // Act
        var response = await _client.GetAsync($"/api/dealerships/{createdDealership!.Id}/analytics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDealershipAnalytics_Unauthorized_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/dealerships/some-id/analytics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDealershipListings_AsDealershipOwner_ReturnsListings()
    {
        // Arrange
        var dealer = await SetupDealershipUserAndGetToken();
        
        // Create a dealership first
        var createRequest = CreateValidDealershipRequest();
        var createResponse = await _client.PostAsJsonAsync("/api/dealerships", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdDealership = await createResponse.Content.ReadFromJsonAsync<DealershipDto>();

        // Act
        var response = await _client.GetAsync($"/api/dealerships/{createdDealership!.Id}/listings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<CarListingDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetDealershipListings_AsNonOwner_ReturnsForbidden()
    {
        // Arrange
        var dealer1 = await SetupDealershipUserAndGetToken();
        
        // Create a dealership
        var createRequest = CreateValidDealershipRequest();
        var createResponse = await _client.PostAsJsonAsync("/api/dealerships", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdDealership = await createResponse.Content.ReadFromJsonAsync<DealershipDto>();

        // Switch to different user
        var dealer2 = await SetupIndividualUserAndGetToken();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", dealer2.Token);

        // Act
        var response = await _client.GetAsync($"/api/dealerships/{createdDealership!.Id}/listings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDealershipListings_Unauthorized_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/dealerships/some-id/listings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDealership_AsOwner_ReturnsDealership()
    {
        // Arrange
        var dealer = await SetupDealershipUserAndGetToken();
        
        // Create a dealership first
        var createRequest = CreateValidDealershipRequest();
        var createResponse = await _client.PostAsJsonAsync("/api/dealerships", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdDealership = await createResponse.Content.ReadFromJsonAsync<DealershipDto>();

        // Act
        var response = await _client.GetAsync($"/api/dealerships/{createdDealership!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DealershipDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdDealership.Id);
        result.Name.Should().Be(createdDealership.Name);
    }

    [Fact]
    public async Task GetDealership_AsNonOwner_ReturnsForbidden()
    {
        // Arrange
        var dealer1 = await SetupDealershipUserAndGetToken();
        
        // Create a dealership
        var createRequest = CreateValidDealershipRequest();
        var createResponse = await _client.PostAsJsonAsync("/api/dealerships", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdDealership = await createResponse.Content.ReadFromJsonAsync<DealershipDto>();

        // Switch to different user
        var dealer2 = await SetupIndividualUserAndGetToken();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", dealer2.Token);

        // Act
        var response = await _client.GetAsync($"/api/dealerships/{createdDealership!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetDealership_Unauthorized_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/dealerships/some-id");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDealerships_AsAuthenticatedUser_ReturnsDealerships()
    {
        // Arrange
        await SetupIndividualUserAndGetToken();

        // Act
        var response = await _client.GetAsync("/api/dealerships");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<DealershipDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetDealerships_Unauthorized_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/dealerships");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateDealership_AsOwner_ReturnsSuccess()
    {
        // Arrange
        var dealer = await SetupDealershipUserAndGetToken();
        
        // Create a dealership first
        var createRequest = CreateValidDealershipRequest();
        var createResponse = await _client.PostAsJsonAsync("/api/dealerships", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdDealership = await createResponse.Content.ReadFromJsonAsync<DealershipDto>();

        // Update the dealership
        var updateRequest = new CreateDealershipRequest
        {
            Name = "Updated Dealership",
            CompanyUniqueNumber = "987654321",
            Location = "Updated Location",
            PhoneNumber = "098-765-4321",
            Website = "https://updateddealership.com",
            Description = "Updated description",
            Street = "Updated Street",
            City = "Tirana",
            State = "Albania",
            PostalCode = "10001",
            Country = "Albania"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/dealerships/{createdDealership!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DealershipDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be(updateRequest.Name);
        result.Location.Should().Be(updateRequest.Location);
        result.PhoneNumber.Should().Be(updateRequest.PhoneNumber);
    }

    [Fact]
    public async Task UpdateDealership_AsNonOwner_ReturnsForbidden()
    {
        // Arrange
        var dealer1 = await SetupDealershipUserAndGetToken();
        
        // Create a dealership
        var createRequest = CreateValidDealershipRequest();
        var createResponse = await _client.PostAsJsonAsync("/api/dealerships", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdDealership = await createResponse.Content.ReadFromJsonAsync<DealershipDto>();

        // Switch to different user
        var dealer2 = await SetupIndividualUserAndGetToken();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", dealer2.Token);

        var updateRequest = new CreateDealershipRequest
        {
            Name = "Updated Dealership",
            CompanyUniqueNumber = "987654321",
            Location = "Updated Location",
            PhoneNumber = "098-765-4321",
            Website = "https://updateddealership.com",
            Description = "Updated description",
            Street = "Updated Street",
            City = "Tirana",
            State = "Albania",
            PostalCode = "10001",
            Country = "Albania"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/dealerships/{createdDealership!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateDealership_Unauthorized_ReturnsUnauthorized()
    {
        // Arrange
        var updateRequest = new CreateDealershipRequest
        {
            Name = "Updated Dealership",
            CompanyUniqueNumber = "987654321",
            Location = "Updated Location",
            PhoneNumber = "098-765-4321",
            Website = "https://updateddealership.com",
            Description = "Updated description",
            Street = "Updated Street",
            City = "Tirana",
            State = "Albania",
            PostalCode = "10001",
            Country = "Albania"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/dealerships/some-id", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
} 
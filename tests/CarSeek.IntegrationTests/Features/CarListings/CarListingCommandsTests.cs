using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using CarSeek.Application.Features.Auth.Common;
using CarSeek.Application.Features.CarListings.DTOs;
using CarSeek.Application.Features.CarListings.Commands;
using CarSeek.Domain.Enums;
using Xunit;

namespace CarSeek.IntegrationTests.Features.CarListings;

public class CarListingCommandsTests : TestBase
{
    public CarListingCommandsTests(CustomWebApplicationFactory factory) : base(factory) { }

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
        
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult!.Token);

        return authResult;
    }

    private CreateCarListingRequest CreateValidCarListingRequest()
    {
        return new CreateCarListingRequest
        {
            Title = "Test Car Listing",
            Description = "A test car listing for integration tests",
            Year = 2020,
            Make = "BMW",
            Model = "X5",
            Price = 50000,
            Mileage = 50000,
            FuelType = "Petrol",
            Transmission = "Automatic",
            Color = "Black",
            Features = "[\"Navigation\", \"Leather Seats\", \"Sunroof\"]"
        };
    }

    [Fact]
    public async Task CreateCarListing_WithValidData_ReturnsSuccess()
    {
        // Arrange
        await SetupIndividualUserAndGetToken();
        var request = CreateValidCarListingRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/api/carlistings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CarListingDto>();
        result.Should().NotBeNull();
        result!.Title.Should().Be(request.Title);
        result.Make.Should().Be(request.Make);
        result.Model.Should().Be(request.Model);
        result.Price.Should().Be(request.Price);
        result.Status.Should().Be(ListingStatus.Active);
    }

    [Fact]
    public async Task CreateCarListing_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        await SetupIndividualUserAndGetToken();
        var request = CreateValidCarListingRequest();
        request.Title = ""; // Invalid: empty title

        // Act
        var response = await _client.PostAsJsonAsync("/api/carlistings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCarListing_Unauthorized_ReturnsUnauthorized()
    {
        // Arrange
        var request = CreateValidCarListingRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/api/carlistings", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateCarListing_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var user = await SetupIndividualUserAndGetToken();
        
        // First create a listing
        var createRequest = CreateValidCarListingRequest();
        var createResponse = await _client.PostAsJsonAsync("/api/carlistings", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdListing = await createResponse.Content.ReadFromJsonAsync<CarListingDto>();

        // Update the listing
        var updateRequest = new UpdateCarListingRequest
        {
            Title = "Updated Car Listing",
            Description = "Updated description",
            Year = 2021,
            Make = "Audi",
            Model = "Q7",
            Price = 60000,
            Mileage = 40000
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/carlistings/{createdListing!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CarListingDto>();
        result.Should().NotBeNull();
        result!.Title.Should().Be(updateRequest.Title);
        result.Make.Should().Be(updateRequest.Make);
        result.Model.Should().Be(updateRequest.Model);
        result.Price.Should().Be(updateRequest.Price);
    }

    [Fact]
    public async Task UpdateCarListing_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var user = await SetupIndividualUserAndGetToken();
        
        // First create a listing
        var createRequest = CreateValidCarListingRequest();
        var createResponse = await _client.PostAsJsonAsync("/api/carlistings", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdListing = await createResponse.Content.ReadFromJsonAsync<CarListingDto>();

        // Update with invalid data
        var updateRequest = new UpdateCarListingRequest
        {
            Title = "", // Invalid: empty title
            Description = "Updated description",
            Year = 2021,
            Make = "Audi",
            Model = "Q7",
            Price = 60000,
            Mileage = 40000
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/carlistings/{createdListing!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCarListing_Unauthorized_ReturnsUnauthorized()
    {
        // Arrange
        var updateRequest = new UpdateCarListingRequest
        {
            Title = "Updated Car Listing",
            Description = "Updated description",
            Year = 2021,
            Make = "Audi",
            Model = "Q7",
            Price = 60000,
            Mileage = 40000
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/carlistings/some-id", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteCarListing_AsOwner_ReturnsSuccess()
    {
        // Arrange
        var user = await SetupIndividualUserAndGetToken();
        
        // First create a listing
        var createRequest = CreateValidCarListingRequest();
        var createResponse = await _client.PostAsJsonAsync("/api/carlistings", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdListing = await createResponse.Content.ReadFromJsonAsync<CarListingDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/carlistings/{createdListing!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteCarListing_AsNonOwner_ReturnsForbidden()
    {
        // Arrange
        var user1 = await SetupIndividualUserAndGetToken();
        
        // First create a listing
        var createRequest = CreateValidCarListingRequest();
        var createResponse = await _client.PostAsJsonAsync("/api/carlistings", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdListing = await createResponse.Content.ReadFromJsonAsync<CarListingDto>();

        // Switch to different user
        var user2 = await SetupDealershipUserAndGetToken();

        // Act
        var response = await _client.DeleteAsync($"/api/carlistings/{createdListing!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteCarListing_Unauthorized_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.DeleteAsync("/api/carlistings/some-id");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateListingStatus_AsOwner_ReturnsSuccess()
    {
        // Arrange
        var user = await SetupIndividualUserAndGetToken();
        
        // First create a listing
        var createRequest = CreateValidCarListingRequest();
        var createResponse = await _client.PostAsJsonAsync("/api/carlistings", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdListing = await createResponse.Content.ReadFromJsonAsync<CarListingDto>();

        var statusRequest = new UpdateListingStatusRequest
        {
            Status = ListingStatus.Sold
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/carlistings/{createdListing!.Id}/status", statusRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CarListingDto>();
        result.Should().NotBeNull();
        result!.Status.Should().Be(ListingStatus.Sold);
    }

    [Fact]
    public async Task UpdateListingStatus_AsNonOwner_ReturnsForbidden()
    {
        // Arrange
        var user1 = await SetupIndividualUserAndGetToken();
        
        // First create a listing
        var createRequest = CreateValidCarListingRequest();
        var createResponse = await _client.PostAsJsonAsync("/api/carlistings", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdListing = await createResponse.Content.ReadFromJsonAsync<CarListingDto>();

        // Switch to different user
        var user2 = await SetupDealershipUserAndGetToken();

        var statusRequest = new UpdateListingStatusRequest
        {
            Status = ListingStatus.Sold
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/carlistings/{createdListing!.Id}/status", statusRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UploadCarImages_WithValidImages_ReturnsSuccess()
    {
        // Arrange
        var user = await SetupIndividualUserAndGetToken();
        
        // First create a listing
        var createRequest = CreateValidCarListingRequest();
        var createResponse = await _client.PostAsJsonAsync("/api/carlistings", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdListing = await createResponse.Content.ReadFromJsonAsync<CarListingDto>();

        // Create test image data
        var imageData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // Minimal JPEG header
        var formData = new MultipartFormDataContent();
        formData.Add(new ByteArrayContent(imageData), "images", "test1.jpg");
        formData.Add(new ByteArrayContent(imageData), "images", "test2.jpg");

        // Act
        var response = await _client.PostAsync($"/api/carlistings/{createdListing!.Id}/images", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<CarImageDto>>();
        result.Should().NotBeNull();
        result!.Count.Should().Be(2);
    }

    [Fact]
    public async Task UploadCarImages_AsNonOwner_ReturnsForbidden()
    {
        // Arrange
        var user1 = await SetupIndividualUserAndGetToken();
        
        // First create a listing
        var createRequest = CreateValidCarListingRequest();
        var createResponse = await _client.PostAsJsonAsync("/api/carlistings", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdListing = await createResponse.Content.ReadFromJsonAsync<CarListingDto>();

        // Switch to different user
        var user2 = await SetupDealershipUserAndGetToken();

        // Create test image data
        var imageData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
        var formData = new MultipartFormDataContent();
        formData.Add(new ByteArrayContent(imageData), "images", "test.jpg");

        // Act
        var response = await _client.PostAsync($"/api/carlistings/{createdListing!.Id}/images", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteCarListingsByUser_AsOwner_ReturnsSuccess()
    {
        // Arrange
        var user = await SetupIndividualUserAndGetToken();
        
        // Create multiple listings
        var createRequest = CreateValidCarListingRequest();
        await _client.PostAsJsonAsync("/api/carlistings", createRequest);
        await _client.PostAsJsonAsync("/api/carlistings", createRequest);

        // Act
        var response = await _client.DeleteAsync("/api/carlistings/my-listings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteCarListingsByUser_Unauthorized_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.DeleteAsync("/api/carlistings/my-listings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserCarListings_AsOwner_ReturnsListings()
    {
        // Arrange
        var user = await SetupIndividualUserAndGetToken();
        
        // Create a listing
        var createRequest = CreateValidCarListingRequest();
        await _client.PostAsJsonAsync("/api/carlistings", createRequest);

        // Act
        var response = await _client.GetAsync("/api/carlistings/my-listings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedCarListingsResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetUserCarListings_Unauthorized_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/carlistings/my-listings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
} 
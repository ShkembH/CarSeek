using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using CarSeek.Application.Features.Auth.Common;
using CarSeek.Application.Features.CarListings.DTOs;
using CarSeek.Domain.Enums;
using Xunit;

namespace CarSeek.IntegrationTests.Features.Search;

public class SearchTests : TestBase
{
    public SearchTests(CustomWebApplicationFactory factory) : base(factory) { }

    private async Task<AuthResponse> SetupUserAndGetToken()
    {
        var registerRequest = new RegisterRequest(
            Email: "search@test.com",
            Password: "Search123!",
            FirstName: "Search",
            LastName: "User",
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

    private async Task<CarListingDto> CreateTestListing(string title, string make, string model, int year, decimal price)
    {
        var user = await SetupUserAndGetToken();

        var createRequest = new CreateCarListingRequest
        {
            Title = title,
            Description = $"A test car listing: {title}",
            Year = year,
            Make = make,
            Model = model,
            Price = price,
            Mileage = 50000,
            FuelType = "Petrol",
            Transmission = "Automatic",
            Color = "Black",
            Features = "[\"Navigation\", \"Leather Seats\"]"
        };

        var response = await _client.PostAsJsonAsync("/api/carlistings", createRequest);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CarListingDto>();
    }

    [Fact]
    public async Task Search_WithNoFilters_ReturnsAllListings()
    {
        // Arrange
        await CreateTestListing("BMW X5 2020", "BMW", "X5", 2020, 50000);
        await CreateTestListing("Audi Q7 2021", "Audi", "Q7", 2021, 60000);

        // Act
        var response = await _client.GetAsync("/api/carlistings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedCarListingsResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Search_WithMakeFilter_ReturnsFilteredResults()
    {
        // Arrange
        await CreateTestListing("BMW X5 2020", "BMW", "X5", 2020, 50000);
        await CreateTestListing("Audi Q7 2021", "Audi", "Q7", 2021, 60000);

        // Act
        var response = await _client.GetAsync("/api/carlistings?make=BMW");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedCarListingsResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Items.All(x => x.Make == "BMW").Should().BeTrue();
    }

    [Fact]
    public async Task Search_WithModelFilter_ReturnsFilteredResults()
    {
        // Arrange
        await CreateTestListing("BMW X5 2020", "BMW", "X5", 2020, 50000);
        await CreateTestListing("BMW X3 2021", "BMW", "X3", 2021, 45000);

        // Act
        var response = await _client.GetAsync("/api/carlistings?model=X5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedCarListingsResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Items.All(x => x.Model == "X5").Should().BeTrue();
    }

    [Fact]
    public async Task Search_WithYearRange_ReturnsFilteredResults()
    {
        // Arrange
        await CreateTestListing("BMW X5 2020", "BMW", "X5", 2020, 50000);
        await CreateTestListing("Audi Q7 2021", "Audi", "Q7", 2021, 60000);
        await CreateTestListing("Mercedes GLE 2019", "Mercedes", "GLE", 2019, 40000);

        // Act
        var response = await _client.GetAsync("/api/carlistings?minYear=2020&maxYear=2021");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedCarListingsResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Items.All(x => x.Year >= 2020 && x.Year <= 2021).Should().BeTrue();
    }

    [Fact]
    public async Task Search_WithPriceRange_ReturnsFilteredResults()
    {
        // Arrange
        await CreateTestListing("BMW X5 2020", "BMW", "X5", 2020, 50000);
        await CreateTestListing("Audi Q7 2021", "Audi", "Q7", 2021, 60000);
        await CreateTestListing("Mercedes GLE 2019", "Mercedes", "GLE", 2019, 40000);

        // Act
        var response = await _client.GetAsync("/api/carlistings?minPrice=45000&maxPrice=55000");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedCarListingsResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Items.All(x => x.Price >= 45000 && x.Price <= 55000).Should().BeTrue();
    }

    [Fact]
    public async Task Search_WithSearchTerm_ReturnsFilteredResults()
    {
        // Arrange
        await CreateTestListing("BMW X5 2020", "BMW", "X5", 2020, 50000);
        await CreateTestListing("Audi Q7 2021", "Audi", "Q7", 2021, 60000);

        // Act
        var response = await _client.GetAsync("/api/carlistings?searchTerm=BMW");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedCarListingsResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Items.All(x => x.Make == "BMW" || x.Title.Contains("BMW")).Should().BeTrue();
    }

    [Fact]
    public async Task Search_WithPagination_ReturnsPaginatedResults()
    {
        // Arrange
        await CreateTestListing("BMW X5 2020", "BMW", "X5", 2020, 50000);
        await CreateTestListing("Audi Q7 2021", "Audi", "Q7", 2021, 60000);
        await CreateTestListing("Mercedes GLE 2019", "Mercedes", "GLE", 2019, 40000);

        // Act
        var response = await _client.GetAsync("/api/carlistings?pageNumber=1&pageSize=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedCarListingsResponse>();
        result.Should().NotBeNull();
        result!.Items.Count().Should().BeLessThanOrEqualTo(2);
        result.PageNumber.Should().Be(1);
        result.TotalPages.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task Search_WithMultipleFilters_ReturnsFilteredResults()
    {
        // Arrange
        await CreateTestListing("BMW X5 2020", "BMW", "X5", 2020, 50000);
        await CreateTestListing("BMW X3 2021", "BMW", "X3", 2021, 45000);
        await CreateTestListing("Audi Q7 2021", "Audi", "Q7", 2021, 60000);

        // Act
        var response = await _client.GetAsync("/api/carlistings?make=BMW&minYear=2020&maxPrice=55000");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedCarListingsResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Items.All(x => x.Make == "BMW" && x.Year >= 2020 && x.Price <= 55000).Should().BeTrue();
    }

    [Fact]
    public async Task Search_WithInvalidPageNumber_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/carlistings?pageNumber=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_WithInvalidPageSize_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/carlistings?pageSize=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_WithInvalidPriceRange_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/carlistings?minPrice=100&maxPrice=50");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_WithInvalidYearRange_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/carlistings?minYear=2021&maxYear=2020");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_WithNoResults_ReturnsEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/carlistings?make=NonExistentMake");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedCarListingsResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Search_WithSorting_ReturnsSortedResults()
    {
        // Arrange
        await CreateTestListing("BMW X5 2020", "BMW", "X5", 2020, 50000);
        await CreateTestListing("Audi Q7 2021", "Audi", "Q7", 2021, 60000);
        await CreateTestListing("Mercedes GLE 2019", "Mercedes", "GLE", 2019, 40000);

        // Act
        var response = await _client.GetAsync("/api/carlistings?sortBy=price&sortOrder=desc");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedCarListingsResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        
        // Check if prices are in descending order
        var prices = result.Items.Select(x => x.Price).ToList();
        prices.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task Search_WithInvalidSortBy_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/carlistings?sortBy=invalidField");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_WithInvalidSortOrder_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/carlistings?sortOrder=invalid");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_WithStatusFilter_ReturnsFilteredResults()
    {
        // Arrange
        await CreateTestListing("BMW X5 2020", "BMW", "X5", 2020, 50000);
        await CreateTestListing("Audi Q7 2021", "Audi", "Q7", 2021, 60000);

        // Act
        var response = await _client.GetAsync("/api/carlistings?status=Active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedCarListingsResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Items.All(x => x.Status == ListingStatus.Active).Should().BeTrue();
    }

    [Fact]
    public async Task Search_WithFuelTypeFilter_ReturnsFilteredResults()
    {
        // Arrange
        await CreateTestListing("BMW X5 2020", "BMW", "X5", 2020, 50000);
        await CreateTestListing("Audi Q7 2021", "Audi", "Q7", 2021, 60000);

        // Act
        var response = await _client.GetAsync("/api/carlistings?fuelType=Petrol");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedCarListingsResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Items.All(x => x.FuelType == "Petrol").Should().BeTrue();
    }

    [Fact]
    public async Task Search_WithTransmissionFilter_ReturnsFilteredResults()
    {
        // Arrange
        await CreateTestListing("BMW X5 2020", "BMW", "X5", 2020, 50000);
        await CreateTestListing("Audi Q7 2021", "Audi", "Q7", 2021, 60000);

        // Act
        var response = await _client.GetAsync("/api/carlistings?transmission=Automatic");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedCarListingsResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.Items.All(x => x.Transmission == "Automatic").Should().BeTrue();
    }
} 
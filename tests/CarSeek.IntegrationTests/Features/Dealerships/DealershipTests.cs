using System.Net;
using System.Net.Http.Json;
using CarSeek.Application.Features.Auth.Common;
using CarSeek.Application.Features.Dealerships.DTOs;
using CarSeek.Domain.Enums;
using Xunit;

namespace CarSeek.IntegrationTests.Features.Dealerships;

public class DealershipTests : TestBase
{
    public DealershipTests(CustomWebApplicationFactory factory) : base(factory) { }

    // Remove the custom Dispose method - let the base class handle it

    [Fact]
    public async Task Create_WithValidDealerUser_ReturnsCreatedDealership()
    {
        // Arrange
        var registerRequest = new RegisterRequest(
            Email: "dealer@example.com",
            Password: "Dealer123!",
            FirstName: "John",
            LastName: "Dealer",
            PhoneNumber: "123-456-7890", // Added missing PhoneNumber parameter
            Country: "Test Country",      // Added missing Country parameter
            City: "Test City",           // Added missing City parameter
            Role: UserRole.Dealership,
            CompanyName: null,
            CompanyUniqueNumber: null,
            Location: null,
            BusinessCertificate: null);

        var authResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        authResponse.EnsureSuccessStatusCode();
        var authResult = await authResponse.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult!.Token);

        var createDealershipRequest = new CreateDealershipRequest
        {
            Name = "Test Dealership",
            Description = "A test dealership",
            Street = "123 Test St",
            City = "Test City",
            State = "Test State",
            PostalCode = "12345",
            Country = "Test Country",
            PhoneNumber = "123-456-7890",
            Website = "https://testdealership.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/dealerships", createDealershipRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var dealership = await response.Content.ReadFromJsonAsync<DealershipDto>();
        Assert.NotNull(dealership);
        Assert.Equal(createDealershipRequest.Name, dealership.Name);
        Assert.Equal(createDealershipRequest.Description, dealership.Description);
        Assert.Equal(createDealershipRequest.Street, dealership.Address.Street);
        Assert.Equal(createDealershipRequest.City, dealership.Address.City);
        Assert.Equal(createDealershipRequest.State, dealership.Address.State);
        Assert.Equal(createDealershipRequest.PostalCode, dealership.Address.PostalCode);
        Assert.Equal(createDealershipRequest.Country, dealership.Address.Country);
        Assert.Equal(createDealershipRequest.PhoneNumber, dealership.PhoneNumber);
        Assert.Equal(createDealershipRequest.Website, dealership.Website);
    }

    [Fact]
    public async Task Create_WithIndividualUser_ReturnsForbidden()
    {
        // Arrange
        var registerRequest = new RegisterRequest(
            Email: "individual@example.com",
            Password: "Individual123!",
            FirstName: "John",
            LastName: "Individual",
            PhoneNumber: null,
            Country: null,
            City: null,
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

        var createDealershipRequest = new CreateDealershipRequest
        {
            Name = "Test Dealership",
            Description = "A test dealership",
            Street = "123 Test St",
            City = "Test City",
            State = "Test State",
            PostalCode = "12345",
            Country = "Test Country",
            PhoneNumber = "123-456-7890",
            Website = "https://testdealership.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/dealerships", createDealershipRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var createDealershipRequest = new CreateDealershipRequest
        {
            Name = "Test Dealership",
            Description = "A test dealership",
            Street = "123 Test St",
            City = "Test City",
            State = "Test State",
            PostalCode = "12345",
            Country = "Test Country",
            PhoneNumber = "123-456-7890",
            Website = "https://testdealership.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/dealerships", createDealershipRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ExistingDealership_ReturnsOk()
    {
        // Arrange
        var registerRequest = new
        {
            Email = "dealer@test.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Role = UserRole.Dealership
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var token = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token!.Token);

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
            Website = "https://test.com"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/dealerships", createRequest);
        var dealership = await createResponse.Content.ReadFromJsonAsync<DealershipDto>();

        // Act
        var response = await _client.GetAsync($"/api/dealerships/{dealership!.Id}");
        var result = await response.Content.ReadFromJsonAsync<DealershipDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(createRequest.Name, result!.Name);
        Assert.Equal(createRequest.Description, result.Description);
    }

    [Fact]
    public async Task GetById_NonExistentDealership_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/dealerships/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithPaginatedResult()
    {
        // Arrange
        var registerRequest = new
        {
            Email = "dealer@test.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Role = UserRole.Dealership
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var token = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token!.Token);

        // Create multiple dealerships
        for (int i = 1; i <= 3; i++)
        {
            var createRequest = new CreateDealershipRequest
            {
                Name = $"Test Dealership {i}",
                Description = $"Test Description {i}",
                Street = $"123 Test St {i}",
                City = "Test City",
                State = "Test State",
                PostalCode = "12345",
                Country = "Test Country",
                PhoneNumber = "123-456-7890",
                Website = "https://test.com"
            };

            await _client.PostAsJsonAsync("/api/dealerships", createRequest);
        }

        // Act
        var response = await _client.GetAsync("/api/dealerships?pageNumber=1&pageSize=2");
        var result = await response.Content.ReadFromJsonAsync<PaginatedDealershipsResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, result!.Items.Count());
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(1, result.PageNumber);
    }

    [Fact]
    public async Task GetAll_WithSearchTerm_ReturnsFilteredResults()
    {
        // Arrange
        var registerRequest = new
        {
            Email = "dealer@test.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            Role = UserRole.Dealership
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new
        {
            Email = registerRequest.Email,
            Password = registerRequest.Password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var token = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token!.Token);

        // Create dealerships with different names
        var dealerships = new[]
        {
            new CreateDealershipRequest { Name = "ABC Motors", Description = "Description", Street = "Address", City = "City", State = "State", PostalCode = "12345", Country = "Country", PhoneNumber = "123-456-7890", Website = "https://abc.com" },
            new CreateDealershipRequest { Name = "XYZ Cars", Description = "Description", Street = "Address", City = "City", State = "State", PostalCode = "12345", Country = "Country", PhoneNumber = "123-456-7890", Website = "https://xyz.com" },
            new CreateDealershipRequest { Name = "ABC Auto", Description = "Description", Street = "Address", City = "City", State = "State", PostalCode = "12345", Country = "Country", PhoneNumber = "123-456-7890", Website = "https://abcauto.com" }
        };

        foreach (var dealership in dealerships)
        {
            await _client.PostAsJsonAsync("/api/dealerships", dealership);
        }

        // Act
        var response = await _client.GetAsync("/api/dealerships?searchTerm=ABC");
        var result = await response.Content.ReadFromJsonAsync<PaginatedDealershipsResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, result!.Items.Count());
        Assert.All(result.Items, item => Assert.Contains("ABC", item.Name));
    }
}

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using CarSeek.Application.Features.Auth.Common;
using CarSeek.Application.Features.Admin.DTOs;
using CarSeek.Application.Features.CarListings.DTOs;
using CarSeek.Application.Features.Dealerships.DTOs;
using CarSeek.Domain.Enums;
using Xunit;

namespace CarSeek.IntegrationTests.Features.Admin;

public class AdminTests : TestBase
{
    public AdminTests(CustomWebApplicationFactory factory) : base(factory) { }

    private async Task<AuthResponse> SetupAdminUserAndGetToken()
    {
        // Register admin user
        var registerRequest = new RegisterRequest(
            Email: "admin@test.com",
            Password: "Admin123!",
            FirstName: "Admin",
            LastName: "User",
            PhoneNumber: "123-456-7890",
            Country: "Kosovo",
            City: "Pristina",
            Role: UserRole.Admin,
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

    private async Task<AuthResponse> SetupRegularUserAndGetToken()
    {
        var registerRequest = new RegisterRequest(
            Email: "user@test.com",
            Password: "User123!",
            FirstName: "Regular",
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
        
        return authResult!;
    }

    [Fact]
    public async Task GetAdminStats_AsAdmin_ReturnsStats()
    {
        // Arrange
        await SetupAdminUserAndGetToken();

        // Act
        var response = await _client.GetAsync("/api/admin/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stats = await response.Content.ReadFromJsonAsync<AdminStatsResponse>();
        stats.Should().NotBeNull();
        stats!.TotalUsers.Should().BeGreaterThanOrEqualTo(1); // At least admin user
        stats.TotalListings.Should().BeGreaterThanOrEqualTo(0);
        stats.TotalDealerships.Should().BeGreaterThanOrEqualTo(0);
        stats.PendingApprovals.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetAdminStats_AsRegularUser_ReturnsForbidden()
    {
        // Arrange
        var regularUser = await SetupRegularUserAndGetToken();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regularUser.Token);

        // Act
        var response = await _client.GetAsync("/api/admin/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAdminStats_Unauthorized_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/admin/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUsers_AsAdmin_ReturnsUsers()
    {
        // Arrange
        await SetupAdminUserAndGetToken();

        // Act
        var response = await _client.GetAsync("/api/admin/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        users.Should().NotBeNull();
        users!.Count.Should().BeGreaterThanOrEqualTo(1); // At least admin user
    }

    [Fact]
    public async Task GetUsers_AsRegularUser_ReturnsForbidden()
    {
        // Arrange
        var regularUser = await SetupRegularUserAndGetToken();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regularUser.Token);

        // Act
        var response = await _client.GetAsync("/api/admin/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAdminListings_AsAdmin_ReturnsListings()
    {
        // Arrange
        await SetupAdminUserAndGetToken();

        // Act
        var response = await _client.GetAsync("/api/admin/listings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var listings = await response.Content.ReadFromJsonAsync<List<CarListingDto>>();
        listings.Should().NotBeNull();
        listings!.Count.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetAdminListings_AsRegularUser_ReturnsForbidden()
    {
        // Arrange
        var regularUser = await SetupRegularUserAndGetToken();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regularUser.Token);

        // Act
        var response = await _client.GetAsync("/api/admin/listings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAdminDealerships_AsAdmin_ReturnsDealerships()
    {
        // Arrange
        await SetupAdminUserAndGetToken();

        // Act
        var response = await _client.GetAsync("/api/admin/dealerships");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dealerships = await response.Content.ReadFromJsonAsync<List<DealershipDto>>();
        dealerships.Should().NotBeNull();
        dealerships!.Count.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetAdminDealerships_AsRegularUser_ReturnsForbidden()
    {
        // Arrange
        var regularUser = await SetupRegularUserAndGetToken();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regularUser.Token);

        // Act
        var response = await _client.GetAsync("/api/admin/dealerships");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPendingApprovals_AsAdmin_ReturnsPendingApprovals()
    {
        // Arrange
        await SetupAdminUserAndGetToken();

        // Act
        var response = await _client.GetAsync("/api/admin/pending-approvals");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pendingApprovals = await response.Content.ReadFromJsonAsync<List<PendingApprovalDto>>();
        pendingApprovals.Should().NotBeNull();
        pendingApprovals!.Count.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetPendingApprovals_AsRegularUser_ReturnsForbidden()
    {
        // Arrange
        var regularUser = await SetupRegularUserAndGetToken();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regularUser.Token);

        // Act
        var response = await _client.GetAsync("/api/admin/pending-approvals");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetRecentActivity_AsAdmin_ReturnsActivityLogs()
    {
        // Arrange
        await SetupAdminUserAndGetToken();

        // Act
        var response = await _client.GetAsync("/api/admin/recent-activity");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var activityLogs = await response.Content.ReadFromJsonAsync<List<ActivityLogDto>>();
        activityLogs.Should().NotBeNull();
        activityLogs!.Count.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetRecentActivity_AsRegularUser_ReturnsForbidden()
    {
        // Arrange
        var regularUser = await SetupRegularUserAndGetToken();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regularUser.Token);

        // Act
        var response = await _client.GetAsync("/api/admin/recent-activity");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ApproveDealership_AsAdmin_ReturnsSuccess()
    {
        // Arrange
        await SetupAdminUserAndGetToken();

        // First create a dealership user
        var dealershipRegisterRequest = new RegisterRequest(
            Email: "dealership@test.com",
            Password: "Dealership123!",
            FirstName: "Dealership",
            LastName: "User",
            PhoneNumber: "123-456-7890",
            Country: "Kosovo",
            City: "Pristina",
            Role: UserRole.Dealership,
            CompanyName: "Test Dealership",
            CompanyUniqueNumber: "123456789",
            Location: "Test Location",
            BusinessCertificate: null);

        await _client.PostAsJsonAsync("/api/auth/register", dealershipRegisterRequest);

        // Act
        var response = await _client.PostAsync("/api/admin/approve-dealership/dealership@test.com", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ApproveDealership_AsRegularUser_ReturnsForbidden()
    {
        // Arrange
        var regularUser = await SetupRegularUserAndGetToken();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regularUser.Token);

        // Act
        var response = await _client.PostAsync("/api/admin/approve-dealership/test@test.com", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RejectDealership_AsAdmin_ReturnsSuccess()
    {
        // Arrange
        await SetupAdminUserAndGetToken();

        // Act
        var response = await _client.PostAsync("/api/admin/reject-dealership/test@test.com", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RejectDealership_AsRegularUser_ReturnsForbidden()
    {
        // Arrange
        var regularUser = await SetupRegularUserAndGetToken();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regularUser.Token);

        // Act
        var response = await _client.PostAsync("/api/admin/reject-dealership/test@test.com", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteUser_AsAdmin_ReturnsSuccess()
    {
        // Arrange
        await SetupAdminUserAndGetToken();

        // First create a user to delete
        var userToDelete = await SetupRegularUserAndGetToken();

        // Act
        var response = await _client.DeleteAsync($"/api/admin/users/{userToDelete.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteUser_AsRegularUser_ReturnsForbidden()
    {
        // Arrange
        var regularUser = await SetupRegularUserAndGetToken();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regularUser.Token);

        // Act
        var response = await _client.DeleteAsync("/api/admin/users/some-user-id");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteListing_AsAdmin_ReturnsSuccess()
    {
        // Arrange
        await SetupAdminUserAndGetToken();

        // Act
        var response = await _client.DeleteAsync("/api/admin/listings/some-listing-id");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteListing_AsRegularUser_ReturnsForbidden()
    {
        // Arrange
        var regularUser = await SetupRegularUserAndGetToken();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", regularUser.Token);

        // Act
        var response = await _client.DeleteAsync("/api/admin/listings/some-listing-id");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
} 
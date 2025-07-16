using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using CarSeek.Application.Common.Models;
using CarSeek.Domain.Enums;
using Xunit;

namespace CarSeek.IntegrationTests.Features.Profile;

public class ProfileTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProfileTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProfile_Unauthorized_IfNotLoggedIn()
    {
        var response = await _client.GetAsync("/api/profile");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAndUpdateProfile_Individual_Success()
    {
        // Register and login as individual
        var register = new
        {
            Email = "indiv1@example.com",
            Password = "Password1!",
            FirstName = "John",
            LastName = "Doe",
            Role = UserRole.Individual
        };
        await _client.PostAsJsonAsync("/api/auth/register", register);
        var login = new { Email = register.Email, Password = register.Password };
        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", login);
        var loginJson = await loginResp.Content.ReadAsStringAsync();
        var token = JsonDocument.Parse(loginJson).RootElement.GetProperty("token").GetString();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Get profile
        var getResp = await _client.GetAsync("/api/profile");
        getResp.EnsureSuccessStatusCode();
        var profile = await getResp.Content.ReadFromJsonAsync<ProfileResponse>();
        profile!.Email.Should().Be(register.Email);
        profile.FirstName.Should().Be(register.FirstName);
        profile.LastName.Should().Be(register.LastName);
        profile.Role.Should().Be(UserRole.Individual.ToString());

        // Update profile
        var update = new MultipartFormDataContent
        {
            { new StringContent("+1234567890"), "PhoneNumber" },
            { new StringContent("Kosovo"), "Country" },
            { new StringContent("Pristina"), "City" },
            { new StringContent("Jane"), "FirstName" },
            { new StringContent("Smith"), "LastName" }
        };
        var putResp = await _client.PutAsync("/api/profile", update);
        putResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Get profile again
        var getResp2 = await _client.GetAsync("/api/profile");
        getResp2.EnsureSuccessStatusCode();
        var profile2 = await getResp2.Content.ReadFromJsonAsync<ProfileResponse>();
        profile2!.FirstName.Should().Be("Jane");
        profile2.LastName.Should().Be("Smith");
        profile2.PhoneNumber.Should().Be("+1234567890");
        profile2.Country.Should().Be("Kosovo");
        profile2.City.Should().Be("Pristina");
    }

    [Fact]
    public async Task GetAndUpdateProfile_Dealership_Success()
    {
        // Register and login as dealership
        var register = new
        {
            Email = "dealer1@example.com",
            Password = "Password1!",
            FirstName = "Dealer",
            LastName = "Owner",
            Role = UserRole.Dealership
        };
        await _client.PostAsJsonAsync("/api/auth/register", register);
        var login = new { Email = register.Email, Password = register.Password };
        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", login);
        var loginJson = await loginResp.Content.ReadAsStringAsync();
        var token = JsonDocument.Parse(loginJson).RootElement.GetProperty("token").GetString();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Simulate dealership creation (if needed)
        // ...

        // Update profile with dealership fields and file
        var update = new MultipartFormDataContent
        {
            { new StringContent("My Company"), "CompanyName" },
            { new StringContent("123456789"), "CompanyUniqueNumber" },
            { new StringContent("Pristina"), "Location" },
            { new StringContent("+1234567890"), "PhoneNumber" },
        };
        // Add a fake PDF file
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("Fake PDF content"));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        update.Add(fileContent, "BusinessCertificate", "certificate.pdf");

        var putResp = await _client.PutAsync("/api/profile", update);
        putResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Get profile again
        var getResp2 = await _client.GetAsync("/api/profile");
        getResp2.EnsureSuccessStatusCode();
        var profile2 = await getResp2.Content.ReadFromJsonAsync<ProfileResponse>();
        profile2!.CompanyName.Should().Be("My Company");
        profile2.CompanyUniqueNumber.Should().Be("123456789");
        profile2.Location.Should().Be("Pristina");
        profile2.BusinessCertificatePath.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateProfile_ValidationError()
    {
        // Register and login as individual
        var register = new
        {
            Email = "indiv2@example.com",
            Password = "Password1!",
            FirstName = "John",
            LastName = "Doe",
            Role = UserRole.Individual
        };
        await _client.PostAsJsonAsync("/api/auth/register", register);
        var login = new { Email = register.Email, Password = register.Password };
        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", login);
        var loginJson = await loginResp.Content.ReadAsStringAsync();
        var token = JsonDocument.Parse(loginJson).RootElement.GetProperty("token").GetString();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Try to update with invalid phone number
        var update = new MultipartFormDataContent
        {
            { new StringContent("invalid-phone"), "PhoneNumber" }
        };
        var putResp = await _client.PutAsync("/api/profile", update);
        putResp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorJson = await putResp.Content.ReadAsStringAsync();
        errorJson.Should().Contain("Phone number format is invalid");
    }
}

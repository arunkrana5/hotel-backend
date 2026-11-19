using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HotelOS.Contracts.Common;
using HotelOS.Contracts.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace HotelOS.IntegrationTests;

public class AuthApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_With_Valid_Credentials_Should_Return_Success_And_Jwt()
    {
        var loginRequest = new LoginRequest("admin", "Admin@HotelOS2026!");
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<ApiResponse<AuthTokenResponse>>();
        Assert.NotNull(content);
        Assert.True(content.Success);
        Assert.NotNull(content.Data?.AccessToken);
        Assert.NotNull(content.Data?.RefreshToken);
        Assert.Equal("admin", content.Data?.User.Username);
        Assert.Contains("SuperAdmin", content.Data?.User.Roles);
    }

    [Fact]
    public async Task Login_With_Invalid_Password_Should_Return_Unauthorized()
    {
        var loginRequest = new LoginRequest("admin", "WrongPassword999!");
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
        Assert.NotNull(content);
        Assert.False(content.Success);
    }

    [Fact]
    public async Task Profile_Without_Authorization_Should_Return_Unauthorized()
    {
        var response = await _client.GetAsync("/api/v1/auth/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Profile_With_Valid_Bearer_Token_Should_Return_Profile_Details()
    {
        var loginRequest = new LoginRequest("admin", "Admin@HotelOS2026!");
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthTokenResponse>>();

        string token = loginResult!.Data!.AccessToken;

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var profileResult = await response.Content.ReadFromJsonAsync<ApiResponse<UserProfileDto>>();
        Assert.NotNull(profileResult);
        Assert.True(profileResult.Success);
        Assert.Equal("admin", profileResult.Data?.Username);
    }
}

using System.Net;
using System.Net.Http.Json;
using WorkoutApi.DTOs;

namespace WorkoutApi.Tests.Controllers.Auth;

public class RegisterTests
{
    [Fact]
    public async Task Register_WithValidData_CreatesAccountAndAllowsLogin()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        var registration = CreateRegistrationRequest();

        var registrationResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            registration);

        Assert.Equal(
            HttpStatusCode.Created,
            registrationResponse.StatusCode);

        var loginResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequestDto
            {
                Email = registration.Email,
                Password = registration.Password
            });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var login = await loginResponse.Content
            .ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));
    }

    [Fact]
    public async Task Register_WithPasswordThatDoesNotMeetIdentityPolicy_ReturnsBadRequest()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        var registration = CreateRegistrationRequest(password: "weak");

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            registration);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsConflict()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        var registration = CreateRegistrationRequest();

        var firstResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            registration);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        var duplicateEmail = CreateRegistrationRequest(email: registration.Email);
        var duplicateResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            duplicateEmail);

        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }

    private static RegisterRequestDto CreateRegistrationRequest(
        string? email = null,
        string? password = null)
    {
        var uniqueId = Guid.NewGuid().ToString("N");

        return new RegisterRequestDto
        {
            UserName = $"newuser_{uniqueId}",
            Email = email ?? $"{uniqueId}@example.com",
            Password = password ?? "StrongPassword123!"
        };
    }
}

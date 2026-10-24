using System.Net;
using System.Net.Http.Json;
using WorkoutApi.DTOs;
using WorkoutApi.Tests.Helpers;

namespace WorkoutApi.Tests.Controllers.Auth;

public class LoginTests
{
    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequestDto
            {
                Email = "missing@example.com",
                Password = UserTestData.Password
            });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

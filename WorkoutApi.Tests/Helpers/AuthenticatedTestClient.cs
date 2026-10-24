using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WorkoutApi.DTOs;
using WorkoutApi.Models;

namespace WorkoutApi.Tests.Helpers;

public sealed class AuthenticatedTestClient : IDisposable
{
    private AuthenticatedTestClient(HttpClient client, ApplicationUser user)
    {
        Client = client;
        User = user;
    }

    public HttpClient Client { get; }

    public ApplicationUser User { get; }

    public static async Task<AuthenticatedTestClient> CreateAsync(
        CustomWebApplicationFactory factory)
    {
        var client = factory.CreateClient();

        try
        {
            using var scope = factory.Services.CreateScope();
            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();
            var user = await UserTestData.CreateAsync(userManager);
            var loginResponse = await client.PostAsJsonAsync(
                "/api/auth/login",
                new LoginRequestDto
                {
                    Email = user.Email!,
                    Password = UserTestData.Password
                });

            loginResponse.EnsureSuccessStatusCode();
            var login = await loginResponse.Content
                .ReadFromJsonAsync<LoginResponseDto>();

            if (login is null || string.IsNullOrWhiteSpace(login.AccessToken))
            {
                throw new InvalidOperationException(
                    "The test login response did not contain an access token.");
            }

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(login.TokenType, login.AccessToken);

            return new AuthenticatedTestClient(client, user);
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    public void Dispose() => Client.Dispose();
}

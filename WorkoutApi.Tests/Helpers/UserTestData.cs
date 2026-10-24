
using Microsoft.AspNetCore.Identity;
using WorkoutApi.Models;

namespace WorkoutApi.Tests.Helpers;

public static class UserTestData
{
    public const string Password = "TestPassword123!";

    public static async Task<ApplicationUser> CreateAsync(
        UserManager<ApplicationUser> userManager,
        string? userName = null)
    {
        var id = Guid.NewGuid().ToString("N");
        var name = userName ?? $"testuser_{id}";

        var user = new ApplicationUser
        {
            UserName = name,
            Email = $"{id}@example.com",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"Impossibile creare l'utente di test: {errors}");
        }

        return user;
    }
}

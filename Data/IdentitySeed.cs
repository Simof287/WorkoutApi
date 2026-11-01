using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WorkoutApi.Models;

namespace WorkoutApi.Data;

public class IdentitySeed
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();
        const string adminRole = "Admin";

        var adminEmail = configuration["AdminSeed:Email"];

        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            return;
        } 
        if(!await roleManager.RoleExistsAsync(adminRole))
        {
            var createRoleResult = await roleManager.CreateAsync(
                    new IdentityRole(adminRole));

            if (!createRoleResult.Succeeded)
            {
               var errors = string.Join(
                "; ",
                createRoleResult.Errors.Select(error => error.Description));

               throw new InvalidOperationException($"Impossibile creare il ruolo {adminRole}: {errors}"); 
            }
    
        }

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if(adminUser is null)
        {
            return;
        }

        if(!await userManager.IsInRoleAsync(adminUser, adminRole))
        {
            var addRoleResult = await userManager.AddToRoleAsync(
                adminUser,
                adminRole);
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    addRoleResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException($"Impossibile assegnare il ruolo {adminRole}: {errors}");
            } 
        }
    }
}

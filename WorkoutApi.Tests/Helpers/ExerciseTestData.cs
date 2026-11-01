using WorkoutApi.Models;
using System.Net.Http.Json;
using WorkoutApi.DTOs;

namespace WorkoutApi.Tests.Helpers;

public static class ExerciseTestData
{
    public static async Task<ExerciseResponseDto> CreateThroughApiAsync(
        CustomWebApplicationFactory factory,
        string name,
        string muscleGroup)
    {
        using var adminClient = await AuthenticatedTestClient
            .CreateAdminAsync(factory);

        var response = await adminClient.Client.PostAsJsonAsync(
            "/api/exercises",
            new CreateExerciseDto
            {
                Name = name,
                MuscleGroup = muscleGroup
            });
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ExerciseResponseDto>()
            ?? throw new InvalidOperationException(
                "Exercise creation returned an empty response.");
    }

    public static Exercise Create(
        string? name,
        string? muscleGroup)
    {
        return new Exercise
        {
            
            Name = name ?? "testName",
            MuscleGroup = muscleGroup ?? "testMuscle"
        };
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WorkoutApi.DTOs;
using WorkoutApi.Tests.Helpers;

namespace WorkoutApi.Tests.Controllers.Workout;

public class DeleteWorkoutTests
{
    [Fact]
    public async Task DeleteWorkout_RemovesWorkoutButKeepsExercise()
    {
        using var factory = new CustomWebApplicationFactory();
        using var testClient = await AuthenticatedTestClient.CreateAsync(factory);
        var client = testClient.Client;
        
        var exerciseResponse = await client.PostAsJsonAsync(
            "/api/exercises",
            new
            {
                name = "Squat",
                muscleGroup = "Legs"
            });

        exerciseResponse.EnsureSuccessStatusCode();

        var exercise = await exerciseResponse.Content
            .ReadFromJsonAsync<ExerciseResponseDto>();

        
        var workoutResponse = await client.PostAsJsonAsync(
            "/api/workouts",
            new
            {
                notes = "Allenamento di prova",
                exercises = new[]
                {
                    new
                    {
                        exerciseId = exercise!.Id,
                        order = 1,
                        sets = new[]
                        {
                            new { repetitions = 8, weight = 50m }
                        }
                    }
                }
            });

        workoutResponse.EnsureSuccessStatusCode();

        var workout = await workoutResponse.Content
            .ReadFromJsonAsync<WorkoutResponseDto>();

        // Elimina l'allenamento.
        var deleteResponse = await client.DeleteAsync(
            $"/api/workouts/{workout!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // L'allenamento non deve più esistere.
        var getWorkoutResponse = await client.GetAsync(
            $"/api/workouts/{workout.Id}");

        Assert.Equal(HttpStatusCode.NotFound, getWorkoutResponse.StatusCode);

        // L'esercizio del catalogo deve esistere ancora.
        var getExerciseResponse = await client.GetAsync(
            $"/api/exercises/{exercise.Id}");

        Assert.Equal(HttpStatusCode.OK, getExerciseResponse.StatusCode);
    }
    [Fact]
    public async Task DeleteWorkout_WhenIdDoesNotExist_ReturnsNotFound()
    {
        using var factory = new CustomWebApplicationFactory();
        using var testClient = await AuthenticatedTestClient.CreateAsync(factory);
        var client = testClient.Client;

        var response = await client.DeleteAsync("/api/workouts/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
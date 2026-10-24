using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkoutApi.Data;
using WorkoutApi.DTOs;
using WorkoutApi.Tests.Helpers;

namespace WorkoutApi.Tests.Controllers.Workout;

public class CreateWorkoutTests
{
    [Fact]
    public async Task CreateWorkout_WithValidData_ReturnsCreated()
    {
        using var factory = new CustomWebApplicationFactory();
        using var testClient = await AuthenticatedTestClient.CreateAsync(factory);
        var client = testClient.Client;

        var exerciseResponse = await client.PostAsJsonAsync(
            "/api/exercises",
            new CreateExerciseDto
            {
                Name = "Pull-up",
                MuscleGroup = "Back"
            });

        Assert.Equal(
            HttpStatusCode.Created,
            exerciseResponse.StatusCode);

        var exercise = await exerciseResponse.Content
            .ReadFromJsonAsync<ExerciseResponseDto>();

        Assert.NotNull(exercise);

        var workoutResponse = await client.PostAsJsonAsync(
            "/api/workouts",
            new
            {
                date = DateTime.UtcNow,
                notes = "Full body",
                exercises = new[]
                {
                    new
                    {
                        exerciseId = exercise.Id,
                        order = 1,
                        sets = new[]
                        {
                            new { repetitions = 8, weight = 20m },
                            new { repetitions = 6, weight = 25m }
                        }
                    }
                }
            });

        Assert.Equal(
            HttpStatusCode.Created,
            workoutResponse.StatusCode);

        Assert.NotNull(workoutResponse.Headers.Location);

        var workout = await workoutResponse.Content
            .ReadFromJsonAsync<WorkoutResponseDto>();
        Assert.NotNull(workout);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkoutContext>();
        var savedWorkout = await db.Workouts
            .SingleAsync(item => item.Id == workout.Id);

        Assert.Equal(testClient.User.Id, savedWorkout.UserId);
    }

    [Fact]
    public async Task CreateWorkout_WithUnknownExercise_ReturnsBadRequest()
    {
        using var factory = new CustomWebApplicationFactory();
        using var testClient = await AuthenticatedTestClient.CreateAsync(factory);
        var client = testClient.Client;

        var response = await client.PostAsJsonAsync(
            "/api/workouts",
            new
            {
                notes = "Invalid workout",
                exercises = new[]
                {
                    new
                    {
                        exerciseId = 999,
                        order = 1,
                        sets = new[]
                        {
                            new { repetitions = 8, weight = 20m }
                        }
                    }
                }
            });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
[Fact]
    public async Task CreateWorkout_WithNoExercises_ReturnsBadRequest()
    {
        using var factory = new CustomWebApplicationFactory();
        using var testClient = await AuthenticatedTestClient.CreateAsync(factory);
        var client = testClient.Client;

        var response = await client.PostAsJsonAsync(
            "/api/workouts",
            new
            {
                notes = "Empty workout",
                exercises = Array.Empty<object>()
            });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
    [Fact]
    public async Task CreateWorkout_WithDuplicateExerciseOrders_ReturnsBadRequest()
    {
        using var factory = new CustomWebApplicationFactory();
        using var testClient = await AuthenticatedTestClient.CreateAsync(factory);
        var client = testClient.Client;
    
        var exerciseResponse = await client.PostAsJsonAsync(
            "/api/exercises",
            new CreateExerciseDto
            {
                Name = "Pull-up",
                MuscleGroup = "Back"
            });
    
        var exercise = await exerciseResponse.Content
            .ReadFromJsonAsync<ExerciseResponseDto>();
    
        Assert.NotNull(exercise);
    
        var response = await client.PostAsJsonAsync(
            "/api/workouts",
            new
            {
                exercises = new[]
                {
                    new
                    {
                        exerciseId = exercise.Id,
                        order = 1,
                        sets = new[]
                        {
                            new { repetitions = 8, weight = 20m }
                        }
                    },
                    new
                    {
                        exerciseId = exercise.Id,
                        order = 1,
                        sets = new[]
                        {
                            new { repetitions = 6, weight = 25m }
                        }
                    }
                }
            });
    
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
    
    [Fact]
    public async Task CreateWorkout_WithNegativeWeight_ReturnsBadRequest()
    {
        using var factory = new CustomWebApplicationFactory();
        using var testClient = await AuthenticatedTestClient.CreateAsync(factory);
        var client = testClient.Client;
    
        var exerciseResponse = await client.PostAsJsonAsync(
            "/api/exercises",
            new CreateExerciseDto
            {
                Name = "Pull-up",
                MuscleGroup = "Back"
            });
    
        var exercise = await exerciseResponse.Content
            .ReadFromJsonAsync<ExerciseResponseDto>();
    
        Assert.NotNull(exercise);
    
        var response = await client.PostAsJsonAsync(
            "/api/workouts",
            new
            {
                exercises = new[]
                {
                    new
                    {
                        exerciseId = exercise.Id,
                        order = 1,
                        sets = new[]
                        {
                            new { repetitions = 8, weight = -5m }
                        }
                    }
                }
            });
    
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkout_WithoutToken_ReturnsUnauthorized()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/workouts",
            new
            {
                notes = "Unauthenticated workout",
                exercises = new[] { new { exerciseId = 1, order = 1, sets = new[] { new { repetitions = 8, weight = 20m } } } }
            });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
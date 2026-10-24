using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using WorkoutApi.DTOs;
using WorkoutApi.Tests.Helpers;

namespace WorkoutApi.Tests.Controllers.Workouts;

public class GetWorkoutsTests
{
    private static async Task<int> CreateExerciseAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/exercises",
            new
            {
                Name = "Squat",
                MuscleGroup = "Gambe"
            });

        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<JsonElement>();

        return result.GetProperty("id").GetInt32();
    }

    private async Task CreateWorkoutAsync(
        HttpClient client,
        int exerciseId,
        DateTime date)
    {
        var response = await client.PostAsJsonAsync(
            "/api/workouts",
            new
            {
                Date = date,
                Notes = "Test workout",
                Exercises = new[]
                {
                    new
                    {
                        ExerciseId = exerciseId,
                        Order = 1,
                        Sets = new[]
                        {
                            new
                            {
                                Repetitions = 10,
                                Weight = 20m
                            }
                        }
                    }
                }
            });

        Assert.True(
            response.IsSuccessStatusCode,
            await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetWorkouts_WithoutParameters_ReturnsFirstPage()
    {
        using var factory = new CustomWebApplicationFactory();
        using var testClient = await AuthenticatedTestClient.CreateAsync(factory);
        var client = testClient.Client;
        var exerciseId = await CreateExerciseAsync(client);

        for (var i = 1; i <= 12; i++)
        {
            await CreateWorkoutAsync(
                client,
                exerciseId,
                new DateTime(2026, 1, i, 10, 0, 0, DateTimeKind.Utc));
        }

        var response = await client.GetAsync("/api/workouts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result =
            await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(1, result.GetProperty("page").GetInt32());
        Assert.Equal(10, result.GetProperty("pageSize").GetInt32());
        Assert.Equal(12, result.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, result.GetProperty("totalPages").GetInt32());
        Assert.Equal(10, result.GetProperty("items").GetArrayLength());
    }

    [Fact]
    public async Task GetWorkouts_SecondPage_ReturnsRemainingItems()
    {
        using var factory = new CustomWebApplicationFactory();
        using var testClient = await AuthenticatedTestClient.CreateAsync(factory);
        var client = testClient.Client;
        var exerciseId = await CreateExerciseAsync(client);

        for (var i = 1; i <= 5; i++)
        {
            await CreateWorkoutAsync(
                client,
                exerciseId,
                new DateTime(2026, 2, i, 10, 0, 0, DateTimeKind.Utc));
        }

        var response = await client.GetAsync(
            "/api/workouts?page=2&pageSize=2");

        var result =
            await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(2, result.GetProperty("page").GetInt32());
        Assert.Equal(2, result.GetProperty("items").GetArrayLength());
        Assert.Equal(5, result.GetProperty("totalCount").GetInt32());
        Assert.Equal(3, result.GetProperty("totalPages").GetInt32());
    }

    [Fact]
    public async Task GetWorkouts_ReturnsWorkoutsOrderedByDateDescending()
    {
        using var factory = new CustomWebApplicationFactory();
        using var testClient = await AuthenticatedTestClient.CreateAsync(factory);
        var client = testClient.Client;
        var exerciseId = await CreateExerciseAsync(client);

        for (var i = 1; i <= 3; i++)
        {
            await CreateWorkoutAsync(
                client,
                exerciseId,
                new DateTime(2026, 3, i, 10, 0, 0, DateTimeKind.Utc));
        }

        var response = await client.GetAsync("/api/workouts");
        var result =
            await response.Content.ReadFromJsonAsync<JsonElement>();

        var dates = result.GetProperty("items")
            .EnumerateArray()
            .Select(item => item.GetProperty("date").GetDateTime())
            .ToArray();

        Assert.Equal(3, dates.Length);
        Assert.True(dates[0] >= dates[1]);
        Assert.True(dates[1] >= dates[2]);
    }

    [Fact]
    public async Task GetWorkouts_IncludesExercisesAndSets()
    {
        using var factory = new CustomWebApplicationFactory();
        using var testClient = await AuthenticatedTestClient.CreateAsync(factory);
        var client = testClient.Client;
        var exerciseId = await CreateExerciseAsync(client);

        await CreateWorkoutAsync(
            client,
            exerciseId,
            new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc));

        var response = await client.GetAsync("/api/workouts");
        var result =
            await response.Content.ReadFromJsonAsync<JsonElement>();

        var workout = result.GetProperty("items")[0];
        var exercises = workout.GetProperty("exercises");

        Assert.NotEmpty(exercises.EnumerateArray());

        var exercise = exercises[0];
        Assert.Equal(
            exerciseId,
            exercise.GetProperty("exerciseId").GetInt32());

        var sets = exercise.GetProperty("sets");
        Assert.Equal(1, sets.GetArrayLength());
        Assert.Equal(10, sets[0].GetProperty("repetitions").GetInt32());
        Assert.Equal(20, sets[0].GetProperty("weight").GetDecimal());
    }

    [Fact]
    public async Task GetWorkouts_OnlyReturnsCurrentUsersWorkouts()
    {
        using var factory = new CustomWebApplicationFactory();
        using var owner = await AuthenticatedTestClient.CreateAsync(factory);
        using var otherUser = await AuthenticatedTestClient.CreateAsync(factory);

        var exerciseId = await CreateExerciseAsync(owner.Client);
        var workoutResponse = await owner.Client.PostAsJsonAsync(
            "/api/workouts",
            new
            {
                notes = "Private workout",
                exercises = new[]
                {
                    new
                    {
                        exerciseId,
                        order = 1,
                        sets = new[] { new { repetitions = 8, weight = 20m } }
                    }
                }
            });
        workoutResponse.EnsureSuccessStatusCode();
        var workout = await workoutResponse.Content
            .ReadFromJsonAsync<JsonElement>();
        var workoutId = workout.GetProperty("id").GetInt32();

        var listResponse = await otherUser.Client.GetAsync("/api/workouts");
        var list = await listResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, list.GetProperty("totalCount").GetInt32());

        var getResponse = await otherUser.Client.GetAsync(
            $"/api/workouts/{workoutId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        var updateResponse = await otherUser.Client.PutAsJsonAsync(
            $"/api/workouts/{workoutId}",
            new UpdateWorkoutDto { Notes = "Changed by another user" });
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);

        var deleteResponse = await otherUser.Client.DeleteAsync(
            $"/api/workouts/{workoutId}");
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    [Theory]
    [InlineData("?page=0")]
    [InlineData("?page=-1")]
    [InlineData("?pageSize=0")]
    [InlineData("?pageSize=101")]
    public async Task GetWorkouts_InvalidPagination_ReturnsBadRequest(
        string query)
    {
        using var factory = new CustomWebApplicationFactory();
        using var testClient = await AuthenticatedTestClient.CreateAsync(factory);
        var response = await testClient.Client.GetAsync($"/api/workouts{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

}
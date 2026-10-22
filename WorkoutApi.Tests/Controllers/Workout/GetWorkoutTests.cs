using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace WorkoutApi.Tests.Controllers.Workouts;

public class GetWorkoutsTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GetWorkoutsTests()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    private async Task<int> CreateExerciseAsync()
    {
        var response = await _client.PostAsJsonAsync(
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
        int exerciseId,
        DateTime date)
    {
        var response = await _client.PostAsJsonAsync(
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
        var exerciseId = await CreateExerciseAsync();

        for (var i = 1; i <= 12; i++)
        {
            await CreateWorkoutAsync(
                exerciseId,
                new DateTime(2026, 1, i, 10, 0, 0, DateTimeKind.Utc));
        }

        var response = await _client.GetAsync("/api/workouts");

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
        var exerciseId = await CreateExerciseAsync();

        for (var i = 1; i <= 5; i++)
        {
            await CreateWorkoutAsync(
                exerciseId,
                new DateTime(2026, 2, i, 10, 0, 0, DateTimeKind.Utc));
        }

        var response = await _client.GetAsync(
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
        var exerciseId = await CreateExerciseAsync();

        for (var i = 1; i <= 3; i++)
        {
            await CreateWorkoutAsync(
                exerciseId,
                new DateTime(2026, 3, i, 10, 0, 0, DateTimeKind.Utc));
        }

        var response = await _client.GetAsync("/api/workouts");
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
        var exerciseId = await CreateExerciseAsync();

        await CreateWorkoutAsync(
            exerciseId,
            new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc));

        var response = await _client.GetAsync("/api/workouts");
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

    [Theory]
    [InlineData("?page=0")]
    [InlineData("?page=-1")]
    [InlineData("?pageSize=0")]
    [InlineData("?pageSize=101")]
    public async Task GetWorkouts_InvalidPagination_ReturnsBadRequest(
        string query)
    {
        var response = await _client.GetAsync($"/api/workouts{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
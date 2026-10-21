using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WorkoutApi.DTOs;

namespace WorkoutApi.Tests.Controllers.Workout;

public class GetWorkoutTests
{
        [Fact]
    public async Task GetWorkout_AfterCreation_ReturnsExercisesAndSets()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

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

        var createResponse = await client.PostAsJsonAsync(
            "/api/workouts",
            new
            {
                notes = "Test workout",
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
            createResponse.StatusCode);

        var created = await createResponse.Content
            .ReadFromJsonAsync<CreatedWorkoutResponse>();

        Assert.NotNull(created);

        var getResponse = await client.GetAsync(
            $"/api/workouts/{created.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        using var json = JsonDocument.Parse(
            await getResponse.Content.ReadAsStringAsync());

        var root = json.RootElement;

        Assert.Equal(
            "Test workout",
            root.GetProperty("notes").GetString());

        var exercises = root.GetProperty("exercises");

        Assert.Equal(1, exercises.GetArrayLength());

        var workoutExercise = exercises[0];

        Assert.Equal(
            "Pull-up",
            workoutExercise.GetProperty("exerciseName").GetString());

        var sets = workoutExercise.GetProperty("sets");

        Assert.Equal(2, sets.GetArrayLength());
        Assert.Equal(
            8,
            sets[0].GetProperty("repetitions").GetInt32());
        Assert.Equal(
            20m,
            sets[0].GetProperty("weight").GetDecimal());
        Assert.Equal(
            6,
            sets[1].GetProperty("repetitions").GetInt32());
        Assert.Equal(
            25m,
            sets[1].GetProperty("weight").GetDecimal());
    }
    [Fact]
    public async Task GetWorkout_WhenIdDoesNotExist_ReturnsNotFound()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/workouts/999");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    private sealed class CreatedWorkoutResponse
    {
    public int Id { get; set; }
    }

}
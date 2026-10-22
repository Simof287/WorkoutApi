using System.Net;
using System.Net.Http.Json;
using Xunit;
using WorkoutApi.DTOs;

namespace WorkoutApi.Tests.Controllers.Exercise;

public class UpdateExerciseTests
{
    [Fact]
    public async Task UpdateExercise_ChangesExercise()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var createDto = new CreateExerciseDto
        {
            Name = "Pull-up",
            MuscleGroup = "Back"
        };

        var createResponse = await client.PostAsJsonAsync(
            "/api/exercises", createDto);

        var created = await createResponse.Content
            .ReadFromJsonAsync<ExerciseResponseDto>();

        Assert.NotNull(created);

        var updateDto = new CreateExerciseDto
        {
            Name = "Chin-up",
            MuscleGroup = "Biceps"
        };

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/exercises/{created.Id}", updateDto);

        Assert.Equal(
            HttpStatusCode.NoContent,
            updateResponse.StatusCode);

        var getResponse = await client.GetAsync(
            $"/api/exercises/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updated = await getResponse.Content
            .ReadFromJsonAsync<ExerciseResponseDto>();

        Assert.NotNull(updated);
        Assert.Equal("Chin-up", updated.Name);
        Assert.Equal("Biceps", updated.MuscleGroup);
    }

    [Fact]
    public async Task UpdateExercise_WhenIdDoesNotExist_ReturnsNotFound()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var dto = new CreateExerciseDto
        {
            Name = "Pull-up",
            MuscleGroup = "Back"
        };

        var response = await client.PutAsJsonAsync(
            "/api/exercises/999", dto);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }    
}
using System.Net;
using System.Net.Http.Json;
using Xunit;
using WorkoutApi.DTOs;
using WorkoutApi.Tests.Helpers;

namespace WorkoutApi.Tests.Controllers.Exercise;

public class DeleteExerciseTests
{
    [Fact]
    public async Task DeleteExercise_RemovesExercise()
    {
        using var factory = new CustomWebApplicationFactory();
        using var testClient = await AuthenticatedTestClient.CreateAdminAsync(factory);
        var client = testClient.Client;

        var dto = new CreateExerciseDto
        {
            Name = "Pull-up",
            MuscleGroup = "Back"
        };

        var createResponse = await client.PostAsJsonAsync(
            "/api/exercises", dto);

        var created = await createResponse.Content
            .ReadFromJsonAsync<ExerciseResponseDto>();

        Assert.NotNull(created);

        var deleteResponse = await client.DeleteAsync(
            $"/api/exercises/{created.Id}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);

        var getResponse = await client.GetAsync(
            $"/api/exercises/{created.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode);
    }
    [Fact]
    public async Task DeleteExercise_WhenIdDoesNotExist_ReturnsNotFound()
    {
        using var factory = new CustomWebApplicationFactory();
        using var testClient = await AuthenticatedTestClient.CreateAdminAsync(factory);
        var client = testClient.Client;

        var response = await client.DeleteAsync("/api/exercises/999");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }    
}

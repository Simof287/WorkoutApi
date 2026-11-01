using System.Net;
using System.Net.Http.Json;
using Xunit;
using WorkoutApi.DTOs;
using WorkoutApi.Tests.Helpers;

namespace WorkoutApi.Tests.Controllers.Exercise;

public class CreateExerciseTests
{
        [Fact]
    public async Task CreateExercise_ReturnsCreatedAndCanBeRetrieved()
    {
        using var factory = new CustomWebApplicationFactory();
        using var testClient = await AuthenticatedTestClient.CreateAdminAsync(factory);
        var client = testClient.Client;

        var dto = new CreateExerciseDto
        {
            Name = "Pull-up",
            MuscleGroup = "Back"
        };

        var response = await client.PostAsJsonAsync(
            "/api/exercises", dto);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content
            .ReadFromJsonAsync<ExerciseResponseDto>();

        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("Pull-up", created.Name);

        var getResponse = await client.GetAsync(
            $"/api/exercises/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }
    [Fact]
    public async Task CreateExercise_WithEmptyName_ReturnsBadRequest()
    {
        using var factory = new CustomWebApplicationFactory();
        using var testClient = await AuthenticatedTestClient.CreateAdminAsync(factory);
        var client = testClient.Client;

        var dto = new CreateExerciseDto
        {
            Name = "",
            MuscleGroup = "Back"
        };

        var response = await client.PostAsJsonAsync(
            "/api/exercises", dto);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}

using System.Net;
using System.Net.Http.Json;
using WorkoutApi.DTOs;
using WorkoutApi.Tests.Helpers;

namespace WorkoutApi.Tests.Controllers.Exercise;

public class ExerciseAuthorizationTests
{
    [Theory]
    [InlineData("POST", "/api/exercises")]
    [InlineData("PUT", "/api/exercises/1")]
    [InlineData("DELETE", "/api/exercises/1")]
    public async Task WriteEndpoints_WithoutAuthentication_ReturnUnauthorized(
        string method,
        string path)
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await SendWriteRequestAsync(client, method, path);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("POST", "/api/exercises")]
    [InlineData("PUT", "/api/exercises/1")]
    [InlineData("DELETE", "/api/exercises/1")]
    public async Task WriteEndpoints_AsAuthenticatedNonAdmin_ReturnForbidden(
        string method,
        string path)
    {
        using var factory = new CustomWebApplicationFactory();
        using var testClient = await AuthenticatedTestClient.CreateAsync(factory);

        var response = await SendWriteRequestAsync(
            testClient.Client,
            method,
            path);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetEndpoints_RemainPublic()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var listResponse = await client.GetAsync("/api/exercises");
        var detailResponse = await client.GetAsync("/api/exercises/1");

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, detailResponse.StatusCode);
    }

    private static Task<HttpResponseMessage> SendWriteRequestAsync(
        HttpClient client,
        string method,
        string path)
    {
        return method switch
        {
            "POST" => client.PostAsJsonAsync(path, new CreateExerciseDto
            {
                Name = "Test exercise",
                MuscleGroup = "Back"
            }),
            "PUT" => client.PutAsJsonAsync(path, new CreateExerciseDto
            {
                Name = "Updated test exercise",
                MuscleGroup = "Back"
            }),
            "DELETE" => client.DeleteAsync(path),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
        };
    }
}

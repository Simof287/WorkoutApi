using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WorkoutApi.DTOs;
using WorkoutApi.Tests.Helpers;

namespace WorkoutApi.Tests;

public class ProblemDetailsContractTests
{
    [Fact]
    public async Task InvalidPagination_ReturnsValidationProblemDetails()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/exercises?page=0");
        using var problem = await AssertProblemAsync(
            response,
            HttpStatusCode.BadRequest,
            "urn:workout-api:problems:validation-error");

        Assert.Equal("Validation failed", problem.RootElement.GetProperty("title").GetString());
        Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty("page", out _));
        Assert.True(problem.RootElement.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task UnauthenticatedRequest_ReturnsProblemDetails()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/workouts");
        using var problem = await AssertProblemAsync(
            response,
            HttpStatusCode.Unauthorized,
            "urn:workout-api:problems:unauthorized");

        Assert.True(problem.RootElement.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task AuthenticatedNonAdminRequest_ReturnsProblemDetailsForbidden()
    {
        using var factory = new CustomWebApplicationFactory();
        using var testClient = await AuthenticatedTestClient.CreateAsync(factory);

        using var response = await testClient.Client.PostAsJsonAsync(
            "/api/exercises",
            new CreateExerciseDto { Name = "Bench press", MuscleGroup = "Chest" });
        using var problem = await AssertProblemAsync(
            response,
            HttpStatusCode.Forbidden,
            "urn:workout-api:problems:forbidden");

        Assert.True(problem.RootElement.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task MissingExercise_ReturnsProblemDetailsNotFound()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/exercises/999999");
        using var problem = await AssertProblemAsync(
            response,
            HttpStatusCode.NotFound,
            "urn:workout-api:problems:not-found");

        Assert.True(problem.RootElement.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task DuplicateRegistration_ReturnsProblemDetailsConflict()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        var email = $"{Guid.NewGuid():N}@example.com";

        var first = await client.PostAsJsonAsync(
            "/api/auth/register",
            Registration(email));
        first.Dispose();

        using var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            Registration(email));
        using var problem = await AssertProblemAsync(
            response,
            HttpStatusCode.Conflict,
            "urn:workout-api:problems:conflict");

        Assert.Equal("Account already exists", problem.RootElement.GetProperty("title").GetString());
        Assert.True(problem.RootElement.TryGetProperty("traceId", out _));
    }

    private static RegisterRequestDto Registration(string email) => new()
    {
        UserName = $"user_{Guid.NewGuid():N}",
        Email = email,
        Password = "StrongPassword123!"
    };

    private static async Task<JsonDocument> AssertProblemAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatus,
        string expectedType)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
        var root = document.RootElement;

        Assert.Equal((int)expectedStatus, root.GetProperty("status").GetInt32());
        Assert.Equal(expectedType, root.GetProperty("type").GetString());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("title").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("traceId").GetString()));

        return document;
    }
}

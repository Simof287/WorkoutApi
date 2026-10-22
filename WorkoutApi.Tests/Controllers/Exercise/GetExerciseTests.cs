using System.Net;
using System.Net.Http.Json;
using Xunit;
using WorkoutApi.DTOs;
using System.Text.Json;

namespace WorkoutApi.Tests.Controllers.Exercise;

public class GetExerciseTests
{
    private static async Task CreateExerciseAsync(
        HttpClient client,
        string name,
        string muscleGroup)
    {
        var response = await client.PostAsJsonAsync(
            "/api/exercises",
            new { name, muscleGroup });

        Assert.True(
            response.IsSuccessStatusCode,
            await response.Content.ReadAsStringAsync());
    }  

    [Fact]
    public async Task GetExercise_WhenIdDoesNotExist_ReturnsNotFound()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/exercises/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    
    [Fact]
    public async Task GetExercises_WithoutParameters_ReturnsFirstPage()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        
        for (var i = 1; i <= 12; i++)
        {
            await CreateExerciseAsync(
                client, $"Exercise {i}", "Petto");
        }
        var response = await client.GetAsync("/api/exercises");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(1, result.GetProperty("page").GetInt32());
        Assert.Equal(10, result.GetProperty("pageSize").GetInt32());
        Assert.Equal(12, result.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, result.GetProperty("totalPages").GetInt32());
        Assert.Equal(10, result.GetProperty("items").GetArrayLength());
        
    }
    [Fact]
    public async Task GetExercises_WithCustomPageSize_ReturnsRequestedNumberOfItems()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
    
        for (var i = 1; i <= 5; i++)
        {
            await CreateExerciseAsync(
                client, $"Exercise {i}", "Schiena");
        }
        var response = await client.GetAsync(
            "/api/exercises?page=1&pageSize=3");
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(3, result.GetProperty("items").GetArrayLength());
        Assert.Equal(5, result.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, result.GetProperty("totalPages").GetInt32());
    
    }
    [Fact]
    public async Task GetExercises_SecondPage_ReturnsDifferentItems()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
    
        for (var i = 1; i <= 5; i++)
        {
            await CreateExerciseAsync(
                client, $"Exercise {i}", "Gambe");
        }
        var firstResponse = await client.GetAsync(
            "/api/exercises?page=1&pageSize=2");
        var secondResponse = await client.GetAsync(
            "/api/exercises?page=2&pageSize=2");
        var firstPage =
            await firstResponse.Content.ReadFromJsonAsync<JsonElement>();
        var secondPage =
            await secondResponse.Content.ReadFromJsonAsync<JsonElement>();
        var firstIds = firstPage.GetProperty("items")
            .EnumerateArray()
            .Select(item => item.GetProperty("id").GetInt32())
            .ToArray();
        var secondIds = secondPage.GetProperty("items")
            .EnumerateArray()
            .Select(item => item.GetProperty("id").GetInt32())
            .ToArray();
        Assert.Equal(2, firstIds.Length);
        Assert.Equal(2, secondIds.Length);
        Assert.Empty(firstIds.Intersect(secondIds));
        Assert.Equal(2, secondPage.GetProperty("page").GetInt32());
    
    }
        [Fact]
    public async Task GetExercises_WithMuscleGroup_FiltersBeforePagination()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
    
        for (var i = 1; i <= 3; i++)
        {
            await CreateExerciseAsync(
                client, $"Chest exercise {i}", "Petto");
        }
        for (var i = 1; i <= 2; i++)
        {
            await CreateExerciseAsync(
                client, $"Back exercise {i}", "Schiena");
        }
        var response = await client.GetAsync(
            "/api/exercises?muscleGroup=Petto&page=2&pageSize=1");
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        var items = result.GetProperty("items");
        Assert.Equal(1, items.GetArrayLength());
        Assert.Equal(3, result.GetProperty("totalCount").GetInt32());
        Assert.Equal(3, result.GetProperty("totalPages").GetInt32());
        Assert.Equal(2, result.GetProperty("page").GetInt32());
        foreach (var item in items.EnumerateArray())
        {
            Assert.Equal(
                "Petto",
                item.GetProperty("muscleGroup").GetString());
        }
        
    }

    [Fact]
    public async Task GetExercises_WithUnknownMuscleGroup_ReturnsEmptyPage()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
    
        await CreateExerciseAsync(client, "Squat", "Gambe");
        var response = await client.GetAsync(
            "/api/exercises?muscleGroup=GruppoInesistente");
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(result.GetProperty("items").EnumerateArray());
        Assert.Equal(0, result.GetProperty("totalCount").GetInt32());
        Assert.Equal(0, result.GetProperty("totalPages").GetInt32());
        
    }

    [Theory]
    [InlineData("?page=0")]
    [InlineData("?page=-1")]
    [InlineData("?pageSize=0")]
    [InlineData("?pageSize=101")]
    public async Task GetExercises_WithInvalidPagination_ReturnsBadRequest(
        string query)
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
    
        var response = await client.GetAsync(
            $"/api/exercises{query}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    
    }
}
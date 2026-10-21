
/*using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WorkoutApi.DTOs;

namespace WorkoutApi.Tests.Controllers;
public class WorkoutsApiTests
{
    [Fact]
    public async Task CreateWorkout_WithValidData_ReturnsCreated()
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
    }

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
    public async Task CreateWorkout_WithUnknownExercise_ReturnsBadRequest()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

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
    public async Task GetWorkout_WhenIdDoesNotExist_ReturnsNotFound()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/workouts/999");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateWorkout_WithNoExercises_ReturnsBadRequest()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

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
public async Task DeleteWorkout_RemovesWorkoutButKeepsExercise()
{
    using var factory = new CustomWebApplicationFactory();
    using var client = factory.CreateClient();

    
    var exerciseResponse = await client.PostAsJsonAsync(
        "/api/exercises",
        new
        {
            name = "Squat",
            muscleGroup = "Legs"
        });

    exerciseResponse.EnsureSuccessStatusCode();

    var exercise = await exerciseResponse.Content
        .ReadFromJsonAsync<ExerciseResponseDto>();

    
    var workoutResponse = await client.PostAsJsonAsync(
        "/api/workouts",
        new
        {
            notes = "Allenamento di prova",
            exercises = new[]
            {
                new
                {
                    exerciseId = exercise!.Id,
                    order = 1,
                    sets = new[]
                    {
                        new { repetitions = 8, weight = 50m }
                    }
                }
            }
        });

    workoutResponse.EnsureSuccessStatusCode();

    var workout = await workoutResponse.Content
        .ReadFromJsonAsync<WorkoutResponseDto>();

    // Elimina l'allenamento.
    var deleteResponse = await client.DeleteAsync(
        $"/api/workouts/{workout!.Id}");

    Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

    // L'allenamento non deve più esistere.
    var getWorkoutResponse = await client.GetAsync(
        $"/api/workouts/{workout.Id}");

    Assert.Equal(HttpStatusCode.NotFound, getWorkoutResponse.StatusCode);

    // L'esercizio del catalogo deve esistere ancora.
    var getExerciseResponse = await client.GetAsync(
        $"/api/exercises/{exercise.Id}");

    Assert.Equal(HttpStatusCode.OK, getExerciseResponse.StatusCode);
}

[Fact]
public async Task UpdateWorkout_ChangesWorkout()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient(); 

        var exerciseResponse = await client.PostAsJsonAsync(
        "/api/exercises",
        new
        {
            name = "Squat",
            muscleGroup = "Legs"
        });
        exerciseResponse.EnsureSuccessStatusCode();

        var exercise = await exerciseResponse.Content
            .ReadFromJsonAsync<ExerciseResponseDto>();


        var workoutResponse = await client.PostAsJsonAsync(
            "/api/workouts",
            new
            {
                
                notes = "Allenamento di prova",
                exercises = new[]
                {
                    new
                    {
                        exerciseId = exercise!.Id,
                        order = 1,
                        sets = new[]
                        {
                            new { repetitions = 8, weight = 50m }
                        }
                    }
                }
            });

        workoutResponse.EnsureSuccessStatusCode();

        var workout = await workoutResponse.Content
            .ReadFromJsonAsync<WorkoutResponseDto>();
        Assert.NotNull(workout);

        var updateDto = new UpdateWorkoutDto
        {
            Notes = "Allenamento aggiornato."
        };
       var updateResponse = await client.PutAsJsonAsync(
            $"/api/workouts/{workout.Id}", updateDto);
        Assert.Equal(   
            HttpStatusCode.NoContent,
            updateResponse.StatusCode);

        var getResponse = await client.GetAsync(
            $"/api/workouts/{workout.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updated = await getResponse.Content
            .ReadFromJsonAsync<WorkoutResponseDto>();

        Assert.NotNull(updated);
        Assert.Equal("Allenamento aggiornato.", updated.Notes);
        
    }

[Fact]
public async Task UpdateWorkout_NoChangesWorkout()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient(); 

        var exerciseResponse = await client.PostAsJsonAsync(
        "/api/exercises",
        new
        {
            name = "Squat",
            muscleGroup = "Legs"
        });
        exerciseResponse.EnsureSuccessStatusCode();

        var exercise = await exerciseResponse.Content
            .ReadFromJsonAsync<ExerciseResponseDto>();


        var workoutResponse = await client.PostAsJsonAsync(
            "/api/workouts",
            new
            {
                
                notes = "Allenamento di prova",
                exercises = new[]
                {
                    new
                    {
                        exerciseId = exercise!.Id,
                        order = 1,
                        sets = new[]
                        {
                            new { repetitions = 8, weight = 50m }
                        }
                    }
                }
            });

        workoutResponse.EnsureSuccessStatusCode();

        var workout = await workoutResponse.Content
            .ReadFromJsonAsync<WorkoutResponseDto>();
        Assert.NotNull(workout);

        var updateDto = new UpdateWorkoutDto
        {
            Notes = null
        };
       var updateResponse = await client.PutAsJsonAsync(
            $"/api/workouts/{workout.Id}", updateDto);
        Assert.Equal(   
            HttpStatusCode.NoContent,
            updateResponse.StatusCode);

        var getResponse = await client.GetAsync(
            $"/api/workouts/{workout.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updated = await getResponse.Content
            .ReadFromJsonAsync<WorkoutResponseDto>();

        Assert.NotNull(updated);
        Assert.Equal("Allenamento di prova", updated.Notes);
        
    }
[Fact]
public async Task UpdateWorkout_PreservesExercisesAndSets()
{
    using var factory = new CustomWebApplicationFactory();
    using var client = factory.CreateClient(); 
    var exerciseResponse = await client.PostAsJsonAsync(
    "/api/exercises",
    new
    {
        name = "Squat",
        muscleGroup = "Legs"
    });
    exerciseResponse.EnsureSuccessStatusCode();
    var exercise = await exerciseResponse.Content
        .ReadFromJsonAsync<ExerciseResponseDto>();
    var workoutResponse = await client.PostAsJsonAsync(
        "/api/workouts",
        new
        {
            
            notes = "Allenamento di prova",
            exercises = new[]
            {
                new
                {
                    exerciseId = exercise!.Id,
                    order = 1,
                    sets = new[]
                    {
                        new { repetitions = 8, weight = 50m },
                        new { repetitions = 10, weight = 45m }
                    }
                }
            }
        });
    workoutResponse.EnsureSuccessStatusCode();
    var workout = await workoutResponse.Content
        .ReadFromJsonAsync<WorkoutResponseDto>();
    Assert.NotNull(workout);
    var updateDto = new UpdateWorkoutDto
    {
        Notes = "Allenamento aggiornato."
    };
    var updateResponse = await client.PutAsJsonAsync(
        $"/api/workouts/{workout.Id}", updateDto);
    Assert.Equal(   
        HttpStatusCode.NoContent,
        updateResponse.StatusCode);
    var getResponse = await client.GetAsync(
        $"/api/workouts/{workout.Id}");
    Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    var updated = await getResponse.Content
        .ReadFromJsonAsync<WorkoutResponseDto>();
    Assert.NotNull(updated);
    Assert.Equal("Allenamento aggiornato.", updated.Notes);

    var workoutExercise = Assert.Single(updated.Exercises);

    Assert.Equal(exercise.Id, workoutExercise.ExerciseId);
    Assert.Equal("Squat", workoutExercise.ExerciseName);
    Assert.Equal(1, workoutExercise.Order);

    
    Assert.Equal(2, workoutExercise.Sets.Count);

    Assert.Collection(
        workoutExercise.Sets,
        set =>
        {
            Assert.Equal(8, set.Repetitions);
            Assert.Equal(50m, set.Weight);
        },
        set =>
        {
            Assert.Equal(10, set.Repetitions);
            Assert.Equal(45m, set.Weight);
        });
}
[Fact]
    public async Task UpdateWorkout_WhenIdDoesNotExist_ReturnsNotFound()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        var dto = new CreateWorkoutDto
        {
            Notes = "Allenamento aggiornato."
        };

        var response = await client.PutAsJsonAsync(
            "/api/workouts/999", dto);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
    
[Fact]
public async Task DeleteWorkout_WhenIdDoesNotExist_ReturnsNotFound()
{
    using var factory = new CustomWebApplicationFactory();
    using var client = factory.CreateClient();

    var response = await client.DeleteAsync("/api/workouts/999");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
}


    private sealed class CreatedWorkoutResponse
    {
        public int Id { get; set; }
    }
}*/

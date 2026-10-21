using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WorkoutApi.DTOs;

namespace WorkoutApi.Tests.Controllers.Workout;

public class UpdateWorkoutTests
{
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
        
    }
using WorkoutApi.Models;

namespace WorkoutApi.Tests.Helpers;

public static class ExerciseTestData
{
    public static Exercise Create(
        string? name,
        string? muscleGroup)
    {
        return new Exercise
        {
            
            Name = name ?? "testName",
            MuscleGroup = muscleGroup ?? "testMuscle"
        };
    }
}

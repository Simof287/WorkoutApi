
using WorkoutApi.Models;

namespace WorkoutApi.Tests.Helpers;

public static class WorkoutTestData
{
    public static Workout Create(
        string userId,
        DateTime? date = null,
        string? notes = "Test workout")
    {
        return new Workout
        {
            UserId = userId,
            Date = date ?? DateTime.UtcNow,
            Notes = notes
        };
    }
}

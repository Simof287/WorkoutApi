using System;

namespace WorkoutApi.Models;

public class Workout
{
    public int Id { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public string? Notes { get; set; }

    public ICollection<WorkoutExercise> Exercises { get; set; }
        = new List<WorkoutExercise>();
}

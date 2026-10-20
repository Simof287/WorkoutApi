using System;

namespace WorkoutApi.Models;

public class WorkoutSet
{
    public int Id { get; set; }

    public int WorkoutExerciseId { get; set; }
    public WorkoutExercise WorkoutExercise { get; set; } = null!;

    public int Repetitions { get; set; }

    public decimal Weight { get; set; }
}

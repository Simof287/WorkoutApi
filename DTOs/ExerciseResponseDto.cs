using System;

namespace WorkoutApi.DTOs;

public class ExerciseResponseDto
{
    public int Id{get; set;}
    public string Name {get; set;} = string.Empty;
    public string MuscleGroup {get; set;} = string.Empty;
}

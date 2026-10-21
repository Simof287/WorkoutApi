
namespace WorkoutApi.DTOs;

public class WorkoutExerciseResponseDto
{
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int Order { get; set; }

    public List<WorkoutSetResponseDto> Sets { get; set; }
        = new();
}


namespace WorkoutApi.DTOs;

public class WorkoutResponseDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string? Notes { get; set; }

    public List<WorkoutExerciseResponseDto> Exercises { get; set; }
        = new();
}

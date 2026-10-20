
using System.ComponentModel.DataAnnotations;

namespace WorkoutApi.DTOs;

public class CreateWorkoutExerciseDto
{
    [Range(1, int.MaxValue)]
    public int ExerciseId { get; set; }

    [Range(1, int.MaxValue)]
    public int Order { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateWorkoutSetDto> Sets { get; set; }
        = new();
}

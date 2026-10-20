
using System.ComponentModel.DataAnnotations;

namespace WorkoutApi.DTOs;

public class CreateWorkoutDto
{
    public DateTime? Date { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateWorkoutExerciseDto> Exercises { get; set; }
        = new();
}


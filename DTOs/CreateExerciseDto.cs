using System.ComponentModel.DataAnnotations;

namespace WorkoutApi.DTOs;

public class CreateExerciseDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string MuscleGroup { get; set; } = string.Empty;

}

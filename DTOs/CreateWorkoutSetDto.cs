
using System.ComponentModel.DataAnnotations;

namespace WorkoutApi.DTOs;

public class CreateWorkoutSetDto
{
    [Range(1, int.MaxValue)]
    public int Repetitions { get; set; }

    [Range(typeof(decimal), "0", "99999.99")]
    public decimal Weight { get; set; }
}

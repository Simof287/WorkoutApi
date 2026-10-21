using System.ComponentModel.DataAnnotations;

namespace WorkoutApi.DTOs;

public class UpdateWorkoutDto
{
    public DateTime? Date { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
}

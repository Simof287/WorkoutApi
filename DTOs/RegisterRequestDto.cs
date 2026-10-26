using System.ComponentModel.DataAnnotations;

namespace WorkoutApi.DTOs;

public class RegisterRequestDto
{
    [Required]
    [StringLength(256, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

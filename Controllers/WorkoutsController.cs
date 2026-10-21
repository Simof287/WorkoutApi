
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkoutApi.Data;
using WorkoutApi.DTOs;
using WorkoutApi.Models;

namespace WorkoutApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkoutsController : ControllerBase
{
    private readonly WorkoutContext _context;

    public WorkoutsController(WorkoutContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<WorkoutResponseDto>> Create(
        CreateWorkoutDto dto,
        CancellationToken cancellationToken)
    {
        var orders = dto.Exercises
            .Select(e => e.Order)
            .ToList();
        
        if (orders.Distinct().Count() != orders.Count)
        {
            return BadRequest(new
            {
                message = "L'ordine degli esercizi deve essere univoco."
            });
        }
        var exerciseIds = dto.Exercises
            .Select(e => e.ExerciseId)
            .Distinct()
            .ToList();

        var existingIds = await _context.Exercises
            .Where(e => exerciseIds.Contains(e.Id))
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        if (existingIds.Count != exerciseIds.Count)
        {
            return BadRequest(new
            {
                message = "Uno o più esercizi non esistono."
            });
        }

        var workout = new Workout
        {
            Date = dto.Date ?? DateTime.UtcNow,
            Notes = dto.Notes,
            Exercises = dto.Exercises.Select(e =>
                new WorkoutExercise
                {
                    ExerciseId = e.ExerciseId,
                    Order = e.Order,
                    Sets = e.Sets.Select(s =>
                        new WorkoutSet
                        {
                            Repetitions = s.Repetitions,
                            Weight = s.Weight
                        }).ToList()
                }).ToList()
        };

        _context.Workouts.Add(workout);

        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = workout.Id },
            new WorkoutResponseDto
            {
                Id = workout.Id,
                Date = workout.Date,
                Notes = workout.Notes
            });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkoutResponseDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var workout = await _context.Workouts
            .AsNoTracking()
            .Include(w => w.Exercises)
                .ThenInclude(e => e.Exercise)
            .Include(w => w.Exercises)
                .ThenInclude(e => e.Sets)
            .FirstOrDefaultAsync(
                w => w.Id == id,
                cancellationToken);

        if (workout is null)
            return NotFound();

        return Ok(new WorkoutResponseDto
        {
            Id = workout.Id,
            Date = workout.Date,
            Notes = workout.Notes,
            Exercises = workout.Exercises
                .OrderBy(e => e.Order)
                .Select(e => new WorkoutExerciseResponseDto
                {
                    ExerciseId = e.ExerciseId,
                    ExerciseName = e.Exercise.Name,
                    Order = e.Order,
                    Sets = e.Sets.Select(s => new WorkoutSetResponseDto
                    {
                        Repetitions = s.Repetitions,
                        Weight = s.Weight
                    }).ToList()

                }).ToList()
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(
        int id,
        UpdateWorkoutDto dto,
        CancellationToken cancellationToken)
    {
        var workout = await _context.Workouts
            .FirstOrDefaultAsync(
                w => w.Id == id,
                cancellationToken);

        if (workout is null)
            return NotFound();
        
        workout.Date = dto.Date ?? workout.Date;
        workout.Notes = dto.Notes ?? workout.Notes;
        

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();

    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var workout = await _context.Workouts
            .FirstOrDefaultAsync(
                w => w.Id == id,
                cancellationToken);

        if (workout is null)
            return NotFound();

        _context.Workouts.Remove(workout);

        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

}


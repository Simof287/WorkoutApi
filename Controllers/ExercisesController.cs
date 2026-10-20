using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkoutApi.Models;
using WorkoutApi.DTOs;
using WorkoutApi.Data;

namespace WorkoutApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExercisesController : ControllerBase
    {
        private readonly WorkoutContext _context;

        public ExercisesController(WorkoutContext context)
        {
            _context = context;
        }

        private static ExerciseResponseDto ToResponseDto(Exercise exercise)
        {
            return new ExerciseResponseDto
            {
                Id = exercise.Id,
                Name = exercise.Name,
                MuscleGroup = exercise.MuscleGroup
            };
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExerciseResponseDto>>> GetAll(string? muscleGroup)
        {
            var exercises = await _context.Exercises
            .Select(e => new ExerciseResponseDto
            {
                Id = e.Id,
                Name = e.Name,
                MuscleGroup = e.MuscleGroup
            })
            .ToListAsync();

            if(muscleGroup == null)
            {
                return Ok(exercises);
            }

            return Ok(exercises.Where(e => e.MuscleGroup == muscleGroup));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExerciseResponseDto>> GetById(int id)
        {
            var exercise = await _context.Exercises.FindAsync(id);
            if(exercise == null)
            {
                return NotFound();
            }
            return Ok(ToResponseDto(exercise));
        }

        [HttpPost]
        public async Task<ActionResult<ExerciseResponseDto>> Create(CreateExerciseDto dto)
        {
            var exercise = new Exercise
            {
                Name = dto.Name,
                MuscleGroup = dto.MuscleGroup
            };

            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById),new { id = exercise.Id },ToResponseDto(exercise));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, CreateExerciseDto dto)
        {
            var exercise = await _context.Exercises.FindAsync(id);
            if(exercise == null)
            {
                return NotFound();
            }
            exercise.Name = dto.Name;
            exercise.MuscleGroup = dto.MuscleGroup;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var exercise = await _context.Exercises.FindAsync(id);

            if (exercise is null)
            {
                return NotFound();
            }

            _context.Exercises.Remove(exercise);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
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
        public async Task<ActionResult<PagedResponseDto<ExerciseResponseDto>>> GetAll(
            [FromQuery] string? muscleGroup,
            CancellationToken cancellationToken,
            [FromQuery, Range(1, int.MaxValue)] int page = 1,
            [FromQuery, Range(1, 100)] int pageSize = 10)
        {
            var query = _context.Exercises.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(muscleGroup))
            {
                query = query.Where(e => e.MuscleGroup == muscleGroup);
            }

            var totalCount = await query
                .CountAsync(cancellationToken);

            var exercises = await query
                .OrderBy(e => e.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new ExerciseResponseDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    MuscleGroup = e.MuscleGroup
                })
                .ToListAsync(cancellationToken);

            var response = new PagedResponseDto<ExerciseResponseDto>
                {
                    Items = exercises,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExerciseResponseDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var exercise = await _context.Exercises.FindAsync(id,cancellationToken);
            if(exercise is null)
            {
                return NotFound();
            }
            return Ok(ToResponseDto(exercise));
        }

        [HttpPost]
        public async Task<ActionResult<ExerciseResponseDto>> Create(
            CreateExerciseDto dto,
            CancellationToken cancellationToken)
        {
            var exercise = new Exercise
            {
                Name = dto.Name,
                MuscleGroup = dto.MuscleGroup
            };

            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetById),new { id = exercise.Id },ToResponseDto(exercise));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(
            int id,
            CancellationToken cancellationToken,
            CreateExerciseDto dto)
        {
            var exercise = await _context.Exercises.FindAsync(id,cancellationToken);
            if(exercise is null)
            {
                return NotFound();
            }
            exercise.Name = dto.Name;
            exercise.MuscleGroup = dto.MuscleGroup;
            await _context.SaveChangesAsync(cancellationToken);
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            var exercise = await _context.Exercises.FindAsync(id,cancellationToken);

            if (exercise is null)
            {
                return NotFound();
            }

            _context.Exercises.Remove(exercise);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
    }
}

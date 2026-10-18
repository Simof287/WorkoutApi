using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorkoutApi.Models;

namespace WorkoutApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExercisesController : ControllerBase
    {
        private static readonly List<Exercise> Exercises =
        [
            new Exercise
            {
                Id = 1,
                Name = "Pull-up",
                MuscleGroup = "Back"
            },
            new Exercise
            {
                Id = 2,
                Name = "Push-up",
                MuscleGroup = "Chest"
            }
        ];

        [HttpGet]
        public ActionResult<IEnumerable<Exercise>> GetAll(string? muscleGroup)
        {
            if(muscleGroup == null)
            {
                return Ok(Exercises);
            }

            return Ok(Exercises.Where(e => e.MuscleGroup == muscleGroup));
        }
        [HttpGet("{id}")]
        public ActionResult<Exercise> GetById(int id)
        {
            var exercise = Exercises.FirstOrDefault(e => e.Id == id);
            if(exercise == null)
            {
                return NotFound();
            }
            return Ok(exercise);
        }
    }
}

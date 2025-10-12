using Microsoft.AspNetCore.Mvc;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;
using System.Linq;

namespace WorkoutManager.Api.Controllers
{
    [ApiController]
    [Route("api/workout-plans/{planId}/training-days/{dayId}/exercises")]
    public class PlanDayExercisesController : ControllerBase
    {
        [HttpPost]
        public ActionResult<CreatedPlanDayExerciseDto> AddExerciseToTrainingDay(int planId, int dayId, [FromBody] AddExerciseToTrainingDayCommand command)
        {
            var plan = WorkoutPlansController._workoutPlanDetails.FirstOrDefault(p => p.Id == planId);
            if (plan == null || plan.IsLocked)
            {
                return Forbid();
            }

            var trainingDay = plan.TrainingDays.FirstOrDefault(td => td.Id == dayId);
            if (trainingDay == null)
            {
                return NotFound();
            }

            var newExerciseId = new Random().Next(100, 200);

            var responseDto = new CreatedPlanDayExerciseDto(
                Id: newExerciseId,
                TrainingDayId: dayId,
                ExerciseId: command.ExerciseId,
                Order: command.Order
            );
            
            // In a real application, we would add this to the trainingDay.Exercises list.
            // For the mock, we just return the created object.

            return CreatedAtAction(null, new { id = newExerciseId }, responseDto);
        }

        [HttpDelete("{planDayExerciseId}")]
        public IActionResult RemoveExerciseFromTrainingDay(int planId, int dayId, int planDayExerciseId)
        {
            var plan = WorkoutPlansController._workoutPlanDetails.FirstOrDefault(p => p.Id == planId);
            if (plan == null || plan.IsLocked)
            {
                return Forbid();
            }

            var trainingDay = plan.TrainingDays.FirstOrDefault(td => td.Id == dayId);
            if (trainingDay == null)
            {
                return NotFound();
            }

            // In a real application, we would check if the exercise exists in the list and remove it.
            // Here we just return success.

            return NoContent();
        }
    }
}

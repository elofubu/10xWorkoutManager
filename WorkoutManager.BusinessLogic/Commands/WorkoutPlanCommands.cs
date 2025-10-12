using MediatR;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Commands;

public record GetWorkoutPlanDetailQuery(int WorkoutPlanId, Guid UserId) : IRequest<WorkoutPlanDetailDto>;

public record CreateWorkoutPlanCommand(string Name, IEnumerable<CreateTrainingDayCommand> TrainingDays);

public record CreateTrainingDayCommand(string Name, int Order);

public record UpdateWorkoutPlanCommand(int Id, string Name, IEnumerable<UpdateTrainingDayOrderCommand> TrainingDays);

public record UpdateTrainingDayOrderCommand(int Id, int Order);

public record UpdateWorkoutPlanPayload(string Name, IEnumerable<UpdateTrainingDayOrderCommand> TrainingDays);

public class AddExerciseToTrainingDayCommand
{
    public int ExerciseId { get; set; }
    public int Order { get; set; }
}

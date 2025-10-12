using MediatR;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Services.Interfaces;

namespace WorkoutManager.BusinessLogic.Commands;

public record GetWorkoutPlanDetailQuery(int WorkoutPlanId, Guid UserId) : IRequest<WorkoutPlanDetailDto>;

public class GetWorkoutPlanDetailQueryHandler : IRequestHandler<GetWorkoutPlanDetailQuery, WorkoutPlanDetailDto>
{
    private readonly IWorkoutPlanService _workoutPlanService;

    public GetWorkoutPlanDetailQueryHandler(IWorkoutPlanService workoutPlanService)
    {
        _workoutPlanService = workoutPlanService;
    }

    public async Task<WorkoutPlanDetailDto> Handle(GetWorkoutPlanDetailQuery request, CancellationToken cancellationToken)
    {
        return await _workoutPlanService.GetWorkoutPlanAsync(request.WorkoutPlanId, request.UserId, cancellationToken);
    }
}

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

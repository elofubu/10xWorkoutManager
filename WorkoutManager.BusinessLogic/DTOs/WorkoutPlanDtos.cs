using WorkoutManager.BusinessLogic.Commands;

namespace WorkoutManager.BusinessLogic.DTOs;

public record CreateTrainingDayDto
{
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
}

public record CreateWorkoutPlanDto
{
    public string Name { get; set; } = string.Empty;
    public IEnumerable<CreateTrainingDayDto> TrainingDays { get; set; } = new List<CreateTrainingDayDto>();
}

public record WorkoutPlanDto(
    int Id,
    string Name,
    DateTime CreatedAt
);

public record CreatedTrainingDayDto(
    int Id,
    string Name,
    int Order
);

public record CreatedWorkoutPlanDto(
    int Id,
    string Name,
    IEnumerable<CreatedTrainingDayDto> TrainingDays
);

public record CreatedPlanDayExerciseDto(
    int Id,
    int TrainingDayId,
    int ExerciseId,
    int Order
);

public record PlanDayExerciseDto(
    int PlanDayExerciseId,
    int ExerciseId,
    string Name,
    int Order
);

public record TrainingDayDto(
    int Id,
    string Name,
    int Order,
    IEnumerable<PlanDayExerciseDto> Exercises
);

public record WorkoutPlanDetailDto(
    int Id,
    string Name,
    bool IsLocked,
    IEnumerable<TrainingDayDto> TrainingDays
);

public record UpdateWorkoutPlanDto(
    string Name,
    List<UpdateTrainingDayOrderCommand> TrainingDays
);

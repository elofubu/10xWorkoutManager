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
    long Id,
    string Name,
    DateTime CreatedAt,
    IEnumerable<TrainingDayDto> TrainingDays
);

public record CreatedTrainingDayDto(
    long Id,
    string Name,
    int Order
);

public record CreatedWorkoutPlanDto(
    long Id,
    string Name,
    IEnumerable<CreatedTrainingDayDto> TrainingDays
);

public record CreatedPlanDayExerciseDto(
    long Id,
    long TrainingDayId,
    long ExerciseId,
    int Order
);

public record PlanDayExerciseDto(
    long PlanDayExerciseId,
    long ExerciseId,
    string Name,
    int Order
);

public record TrainingDayDto(
    long Id,
    string Name,
    int Order,
    IEnumerable<PlanDayExerciseDto> Exercises
);

public record WorkoutPlanDetailDto(
    long Id,
    string Name,
    bool IsLocked,
    ICollection<TrainingDayDto> TrainingDays
);

public record UpdateWorkoutPlanDto(
    string Name,
    List<UpdateTrainingDayOrderCommand> TrainingDays
);

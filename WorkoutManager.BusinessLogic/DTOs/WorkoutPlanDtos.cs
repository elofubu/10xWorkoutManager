namespace WorkoutManager.BusinessLogic.DTOs;

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

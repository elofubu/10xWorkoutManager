namespace WorkoutManager.BusinessLogic.Commands;

public record CreateWorkoutPlanCommand(string Name, IEnumerable<CreateTrainingDayCommand> TrainingDays);

public record CreateTrainingDayCommand(string Name, int Order);

public record UpdateTrainingDayOrderCommand(long Id, int Order);

public record UpdateWorkoutPlanPayload(string Name, IEnumerable<UpdateTrainingDayOrderCommand> TrainingDays);

public class AddExerciseToTrainingDayCommand
{
    public int ExerciseId { get; set; }
    public int Order { get; set; }
}

public record ReorderExerciseCommand(long Id, int Order);
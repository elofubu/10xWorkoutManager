namespace WorkoutManager.BusinessLogic.Commands;

public class CreateExerciseCommand
{
    public string Name { get; set; } = string.Empty;
    public int MuscleGroupId { get; set; }
}

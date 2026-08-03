namespace WorkoutManager.BusinessLogic.Commands;

public class StartSessionCommand
{
    public int TrainingDayId { get; set; }
}

public class UpdateSessionCommand
{
    public string? Notes { get; set; }
    public DateTime? EndTime { get; set; }
}

public class UpdateSessionExerciseCommand
{
    public string? Notes { get; set; }
    public bool Skipped { get; set; }
    public List<UpdateExerciseSetDto> Sets { get; set; } = new();
}

public class UpdateExerciseSetDto
{
    public decimal Weight { get; set; }
    public int Reps { get; set; }
    public bool IsFailure { get; set; }
    public int Order { get; set; }
}

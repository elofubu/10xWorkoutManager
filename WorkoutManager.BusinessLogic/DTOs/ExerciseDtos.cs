namespace WorkoutManager.BusinessLogic.DTOs;

public record CreateExerciseDto
{
    public string Name { get; set; } = string.Empty;
    public long MuscleGroupId { get; set; }
}

public class ExerciseDto
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public long MuscleGroupId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class PreviousExercisePerformanceDto
{
    public DateTime SessionDate { get; set; }
    public string? Notes { get; set; }
    public List<PreviousExerciseSetDto> Sets { get; set; } = new();
}

public class PreviousExerciseSetDto
{
    public decimal Weight { get; set; }
    public int Reps { get; set; }
    public bool IsFailure { get; set; }
}

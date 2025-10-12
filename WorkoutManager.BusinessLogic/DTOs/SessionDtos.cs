namespace WorkoutManager.BusinessLogic.DTOs;

public class SessionSummaryDto
{
    public int Id { get; set; }
    public int PlanId { get; set; }
    public string? PlanName { get; set; }
    public string? TrainingDayName { get; set; }
    public string? Notes { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

public class SessionDetailsDto
{
    public int Id { get; set; }
    public string? Notes { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public List<SessionExerciseDetailsDto> Exercises { get; set; } = new();
}

public class SessionExerciseDetailsDto
{
    public int Id { get; set; }
    public int ExerciseId { get; set; }
    public string? Notes { get; set; }
    public bool Skipped { get; set; }
    public int Order { get; set; }
    public List<ExerciseSetDto> Sets { get; set; } = new();
}

public class UpdateSessionExerciseDto
{
    public string? Notes { get; set; }
    public bool? Skipped { get; set; }
    public List<ExerciseSetDto> Sets { get; set; } = new();
}

public class ExerciseSetDto
{
    public int Id { get; set; }
    public decimal Weight { get; set; }
    public int Reps { get; set; }
    public bool IsFailure { get; set; }
    public int Order { get; set; }
}

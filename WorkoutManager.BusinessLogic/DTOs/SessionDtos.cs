namespace WorkoutManager.BusinessLogic.DTOs;

public class SessionSummaryDto
{
    public long Id { get; set; }
    public long? PlanId { get; set; }
    public long? TrainingDayId { get; set; }
    public string? PlanName { get; set; }
    public string? TrainingDayName { get; set; }
    public string? Notes { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public List<SessionExerciseDetailsDto> Exercises { get; set; } = new();
}

public class SessionDetailsDto
{
    public long Id { get; set; }
    public long? PlanId { get; set; }
    public long? TrainingDayId { get; set; }
    public string? PlanName { get; set; }
    public string? TrainingDayName { get; set; }
    public string? Notes { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public List<SessionExerciseDetailsDto> Exercises { get; set; } = new();
}

public class SessionExerciseDetailsDto
{
    public long Id { get; set; }
    public long ExerciseId { get; set; }
    public string ExerciseName { get; set; }
    public string? Notes { get; set; }
    public bool Skipped { get; set; }
    public int Order { get; set; }
    public List<ExerciseSetDto> Sets { get; set; }

    public SessionExerciseDetailsDto()
    {
        Sets = new List<ExerciseSetDto>
        {
            new ExerciseSetDto(), new ExerciseSetDto(), new ExerciseSetDto()
        };
    }
}

public class UpdateSessionExerciseDto
{
    public string? Notes { get; set; }
    public bool? Skipped { get; set; }
    public List<ExerciseSetDto> Sets { get; set; } = new();
}

public class ExerciseSetDto
{
    public long Id { get; set; }
    public decimal Weight { get; set; }
    public int Reps { get; set; }
    public bool IsFailure { get; set; }
    public int Order { get; set; }
}

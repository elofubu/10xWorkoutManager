namespace WorkoutManager.Data.Models;

public class TrainingDay
{
    public long Id { get; set; }

    public long PlanId { get; set; }

    public string Name { get; set; } = string.Empty;

    public short Order { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>Many-to-many to Exercise, through <see cref="PlanDayExercise"/>.</summary>
    public List<Exercise> Exercises { get; set; } = new();

    /// <summary>Join entity, exposes the per-day ordering.</summary>
    public List<PlanDayExercise> PlanDayExercises { get; set; } = new();
}

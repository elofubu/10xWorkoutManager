namespace WorkoutManager.Data.Models;

public class Session
{
    public long Id { get; set; }

    public Guid UserId { get; set; }

    public long? PlanId { get; set; }

    public long? TrainingDayId { get; set; }

    public string? Notes { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public WorkoutPlan? Plan { get; set; }

    public TrainingDay? TrainingDay { get; set; }

    public List<SessionExercise> SessionExercises { get; set; } = new();
}

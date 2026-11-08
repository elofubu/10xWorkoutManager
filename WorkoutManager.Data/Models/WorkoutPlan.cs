namespace WorkoutManager.Data.Models;

public class WorkoutPlan
{
    public long Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public List<TrainingDay> TrainingDays { get; set; } = new();
}

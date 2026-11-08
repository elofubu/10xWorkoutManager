namespace WorkoutManager.Data.Models;

public class Exercise
{
    public long Id { get; set; }

    /// <summary>Null for predefined (seeded) exercises, otherwise the owning user.</summary>
    public Guid? UserId { get; set; }

    public long MuscleGroupId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

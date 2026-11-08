namespace WorkoutManager.Data.Models;

public class ExerciseSet
{
    public long Id { get; set; }

    public long SessionExerciseId { get; set; }

    public decimal Weight { get; set; }

    public short Reps { get; set; }

    public bool IsFailure { get; set; }

    public short Order { get; set; }

    public DateTime CreatedAt { get; set; }
}

namespace WorkoutManager.Data.Models;

public class SessionExercise
{
    public long Id { get; set; }

    public long SessionId { get; set; }

    public long ExerciseId { get; set; }

    public string? Notes { get; set; }

    public bool Skipped { get; set; }

    public short Order { get; set; }

    public Session? Session { get; set; }

    public Exercise? Exercise { get; set; }

    public List<ExerciseSet> Sets { get; set; } = new();
}

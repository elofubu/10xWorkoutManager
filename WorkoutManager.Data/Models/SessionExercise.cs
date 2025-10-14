namespace WorkoutManager.Data.Models;

using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("session_exercises")]
public class SessionExercise : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("session_id")]
    public long SessionId { get; set; }

    [Column("exercise_id")]
    public long ExerciseId { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("skipped")]
    public bool Skipped { get; set; }

    [Column("order")]
    public short Order { get; set; }
}

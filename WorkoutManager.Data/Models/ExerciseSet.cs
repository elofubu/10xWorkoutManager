namespace WorkoutManager.Data.Models;

using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("exercise_sets")]
public class ExerciseSet : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("session_exercise_id")]
    public long SessionExerciseId { get; set; }

    [Column("weight")]
    public decimal Weight { get; set; }

    [Column("reps")]
    public short Reps { get; set; }

    [Column("is_failure")]
    public bool IsFailure { get; set; }

    [Column("order")]
    public short Order { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}

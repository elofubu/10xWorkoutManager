namespace WorkoutManager.Data.Models;

using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Collections.Generic;
using Newtonsoft.Json;

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

    [JsonIgnore]
    //[Reference(typeof(Session), useInnerJoin: false, includeInQuery: true)]
    public Session? Session { get; set; }

    [JsonIgnore]
    public Exercise Exercise { get; set; }

    //[JsonIgnore]
    [Reference(typeof(ExerciseSet), useInnerJoin: false, includeInQuery: true)]
    public List<ExerciseSet> Sets { get; set; }
}

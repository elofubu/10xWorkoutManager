namespace WorkoutManager.Data.Models;

using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;

[Table("sessions")]
public class Session : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("plan_id")]
    public long? PlanId { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("start_time")]
    public DateTime StartTime { get; set; }

    [Column("end_time")]
    public DateTime? EndTime { get; set; }

    [JsonIgnore]
    //[JsonProperty("workout_plans")]
    public WorkoutPlan? Plan { get; set; }

    [Reference(typeof(SessionExercise), useInnerJoin: false, includeInQuery: true)]
    public List<SessionExercise> SessionExercises { get; set; } = new();
}

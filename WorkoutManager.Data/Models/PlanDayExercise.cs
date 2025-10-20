namespace WorkoutManager.Data.Models;

using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Newtonsoft.Json;

[Table("plan_day_exercises")]
public class PlanDayExercise : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [PrimaryKey("training_day_id")]
    public long TrainingDayId { get; set; }

    [PrimaryKey("exercise_id")]
    public long ExerciseId { get; set; }

    [Column("order")]
    public short Order { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Reference(typeof(Exercise))]
    public Exercise Exercise { get; set; }

    [Reference(typeof(TrainingDay))]
    public TrainingDay TrainingDay { get; set; }
}

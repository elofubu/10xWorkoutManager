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

    [Column("training_day_id")]
    public long TrainingDayId { get; set; }

    [Column("exercise_id")]
    public long ExerciseId { get; set; }

    [Column("order")]
    public short Order { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonIgnore]
    public Exercise? Exercise { get; set; }
    
    [JsonIgnore]
    public TrainingDay? TrainingDay { get; set; }
}

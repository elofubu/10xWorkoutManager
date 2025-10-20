namespace WorkoutManager.Data.Models;

using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Collections.Generic;
using Newtonsoft.Json;

[Table("training_days")]
public class TrainingDay : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("plan_id")]
    public long PlanId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("order")]
    public short Order { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    // Direct M2M to Exercise for simple queries
    [JsonIgnore]
    //[Reference(typeof(Exercise), useInnerJoin: false, includeInQuery: true)]
    public List<Exercise> Exercises { get; set; } = new();

    // Junction table for order/metadata access - preserves the order column
    //[JsonIgnore]
    [Reference(typeof(PlanDayExercise), useInnerJoin: false, includeInQuery: true)]
    public List<PlanDayExercise> PlanDayExercises { get; set; } = new();
}

namespace WorkoutManager.Data.Models;

using System;
using System.Collections.Generic;
using Postgrest.Attributes;
using Postgrest.Models;

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

    [Reference(typeof(PlanDayExercise))]
    public List<PlanDayExercise> PlanDayExercises { get; set; } = new List<PlanDayExercise>();
}

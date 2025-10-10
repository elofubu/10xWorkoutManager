namespace WorkoutManager.Data.Models;

using System;
using System.Collections.Generic;
using Postgrest.Attributes;
using Postgrest.Models;

[Table("workout_plans")]
public class WorkoutPlan : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Reference(typeof(TrainingDay))]
    public List<TrainingDay> TrainingDays { get; set; } = new List<TrainingDay>();
}

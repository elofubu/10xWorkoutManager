namespace WorkoutManager.Data.Models;

using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

[Table("exercises")]
public class Exercise : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("user_id")]
    public Guid? UserId { get; set; }

    [Column("muscle_group_id")]
    public long MuscleGroupId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}

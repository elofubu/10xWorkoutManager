namespace WorkoutManager.Data.Models;

using Postgrest.Attributes;
using Postgrest.Models;

[Table("muscle_groups")]
public class MuscleGroup : BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}

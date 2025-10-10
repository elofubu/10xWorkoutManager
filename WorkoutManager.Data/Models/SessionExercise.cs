using System.Collections.Generic;
using Postgrest.Attributes;
using Postgrest.Models;

namespace WorkoutManager.Data.Models
{
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

        [Reference(typeof(Exercise))]
        public Exercise Exercise { get; set; }

        [Reference(typeof(Session))]
        public Session Session { get; set; }

        [Reference(typeof(ExerciseSet))]
        public List<ExerciseSet> Sets { get; set; } = new List<ExerciseSet>();
    }
}

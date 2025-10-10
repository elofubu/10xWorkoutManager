namespace WorkoutManager.Business.DTOs
{
    public class SessionDto
    {
        public long Id { get; set; }
        public long? PlanId { get; set; }
        public string? Notes { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public IEnumerable<SessionExerciseDto> Exercises { get; set; } = new List<SessionExerciseDto>();
    }

    public class SessionExerciseDto
    {
        public long Id { get; set; }
        public long ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool Skipped { get; set; }
        public short Order { get; set; }
        public IEnumerable<ExerciseSetDto> Sets { get; set; } = new List<ExerciseSetDto>();
    }
}

namespace WorkoutManager.Business.DTOs
{
    public class ExerciseSetDto
    {
        public long Id { get; set; }
        public long SessionExerciseId { get; set; }
        public decimal Weight { get; set; }
        public short Reps { get; set; }
        public bool IsFailure { get; set; }
        public short Order { get; set; }
    }
}

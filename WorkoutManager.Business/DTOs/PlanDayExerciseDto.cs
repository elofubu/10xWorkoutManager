namespace WorkoutManager.Business.DTOs
{
    public class PlanDayExerciseDto
    {
        public long Id { get; set; }
        public long TrainingDayId { get; set; }
        public long ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public short Order { get; set; }
    }
}

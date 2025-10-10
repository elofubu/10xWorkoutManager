namespace WorkoutManager.Business.DTOs
{
    public class TrainingDayDto
    {
        public long Id { get; set; }
        public long PlanId { get; set; }
        public string Name { get; set; } = string.Empty;
        public short Order { get; set; }
        public List<PlanDayExerciseDto> PlanDayExercises { get; set; } = new List<PlanDayExerciseDto>();
    }
}

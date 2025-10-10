namespace WorkoutManager.Business.DTOs
{
    public class SessionSummaryDto
    {
        public long Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int ExerciseCount { get; set; }
    }
}

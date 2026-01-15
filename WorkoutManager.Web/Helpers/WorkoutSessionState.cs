using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.Web.Helpers
{
    public class WorkoutSessionState
    {
        public long SessionId { get; set; }
        public long CurrentExerciseId { get; set; }
        public int CurrentIndex { get; set; }
        public List<ExerciseSetDto> UnsavedSets { get; set; } = new();
        public int? InputReps { get; set; }
        public decimal? InputWeight { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

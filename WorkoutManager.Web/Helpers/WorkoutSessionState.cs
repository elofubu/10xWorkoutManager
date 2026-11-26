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
        public string? UnsavedNotes { get; set; }

        // Exercise pinned next to the current one (superset); all null when nothing is pinned.
        public long? PinnedExerciseId { get; set; }
        public List<ExerciseSetDto>? PinnedSets { get; set; }
        public int? PinnedInputReps { get; set; }
        public decimal? PinnedInputWeight { get; set; }
        public string? PinnedNotes { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

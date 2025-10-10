using System;

namespace WorkoutManager.Business.DTOs
{
    public class ExerciseHistoryDto
    {
        public DateTime SessionDate { get; set; }
        public decimal Weight { get; set; }
        public short Reps { get; set; }
        public bool IsFailure { get; set; }
    }
}

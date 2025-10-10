using System.ComponentModel.DataAnnotations;

namespace WorkoutManager.Business.DTOs
{
    public class CreateExerciseDto
    {
        [Required]
        public long MuscleGroupId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}

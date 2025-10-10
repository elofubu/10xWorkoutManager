using System.ComponentModel.DataAnnotations;

namespace WorkoutManager.Business.DTOs
{
    public class ExerciseDto
    {
        public long Id { get; set; }

        [Required]
        public long MuscleGroupId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}

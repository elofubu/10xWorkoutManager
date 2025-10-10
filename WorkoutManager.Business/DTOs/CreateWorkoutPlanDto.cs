using System.ComponentModel.DataAnnotations;

namespace WorkoutManager.Business.DTOs
{
    public class CreateWorkoutPlanDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}

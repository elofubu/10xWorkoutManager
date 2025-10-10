using System.ComponentModel.DataAnnotations;

namespace WorkoutManager.Business.DTOs
{
    public class WorkoutPlanDto
    {
        public long Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public List<TrainingDayDto> TrainingDays { get; set; } = new List<TrainingDayDto>();
    }
}

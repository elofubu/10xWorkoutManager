namespace WorkoutManager.BusinessLogic.DTOs;

/// <summary>
/// DTO for exercise details within a training day, including order information.
/// Used to preserve the M2M relationship ordering through the plan_day_exercises junction table.
/// </summary>
public class PlanExerciseDetailDto
{
    /// <summary>
    /// The exercise ID.
    /// </summary>
    public long ExerciseId { get; set; }

    /// <summary>
    /// The exercise name.
    /// </summary>
    public string ExerciseName { get; set; } = string.Empty;

    /// <summary>
    /// The muscle group ID this exercise targets.
    /// </summary>
    public long MuscleGroupId { get; set; }

    /// <summary>
    /// The muscle group name this exercise targets.
    /// </summary>
    public string MuscleGroupName { get; set; } = string.Empty;

    /// <summary>
    /// The order of this exercise within the training day.
    /// This is critical for UI rendering and drag-and-drop operations.
    /// </summary>
    public short Order { get; set; }
}

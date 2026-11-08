namespace WorkoutManager.Data.Models;

public class PlanDayExercise
{
    public long Id { get; set; }

    public long TrainingDayId { get; set; }

    public long ExerciseId { get; set; }

    public short Order { get; set; }

    public DateTime CreatedAt { get; set; }

    public Exercise? Exercise { get; set; }

    public TrainingDay? TrainingDay { get; set; }
}

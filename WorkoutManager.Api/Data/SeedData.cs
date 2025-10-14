namespace WorkoutManager.Api.Data;

using System.Collections.Generic;
using WorkoutManager.Data.Models;

public class ExerciseSeed
{
    public string Name { get; set; } = string.Empty;
    public string MuscleGroupName { get; set; } = string.Empty;
}
public static class SeedData
{
    public static IEnumerable<MuscleGroup> MuscleGroups =>
        new List<MuscleGroup>
        {
            new() { Name = "Chest" },
            new() { Name = "Back" },
            new() { Name = "Shoulders" },
            new() { Name = "Biceps" },
            new() { Name = "Triceps" },
            new() { Name = "Legs" },
            new() { Name = "Abs" },
            new() { Name = "Calves" }
        };

    public static IEnumerable<ExerciseSeed> Exercises =>
        new List<ExerciseSeed>
        {
            new() { Name = "Bench Press", MuscleGroupName = "Chest" },
            new() { Name = "Dumbbell Flyes", MuscleGroupName = "Chest" },
            new() { Name = "Push-ups", MuscleGroupName = "Chest" },
            new() { Name = "Incline Bench Press", MuscleGroupName = "Chest" },
            new() { Name = "Pull-ups", MuscleGroupName = "Back" },
            new() { Name = "Deadlift", MuscleGroupName = "Back" },
            new() { Name = "Bent-over Row", MuscleGroupName = "Back" },
            new() { Name = "Lat Pulldown", MuscleGroupName = "Back" },
            new() { Name = "Overhead Press", MuscleGroupName = "Shoulders" },
            new() { Name = "Lateral Raises", MuscleGroupName = "Shoulders" },
            new() { Name = "Face Pulls", MuscleGroupName = "Shoulders" },
            new() { Name = "Bicep Curls", MuscleGroupName = "Biceps" },
            new() { Name = "Hammer Curls", MuscleGroupName = "Biceps" },
            new() { Name = "Chin-ups", MuscleGroupName = "Biceps" },
            new() { Name = "Tricep Dips", MuscleGroupName = "Triceps" },
            new() { Name = "Skull Crushers", MuscleGroupName = "Triceps" },
            new() { Name = "Tricep Pushdowns", MuscleGroupName = "Triceps" },
            new() { Name = "Squats", MuscleGroupName = "Legs" },
            new() { Name = "Lunges", MuscleGroupName = "Legs" },
            new() { Name = "Leg Press", MuscleGroupName = "Legs" },
            new() { Name = "Romanian Deadlift", MuscleGroupName = "Legs" },
            new() { Name = "Crunches", MuscleGroupName = "Abs" },
            new() { Name = "Leg Raises", MuscleGroupName = "Abs" },
            new() { Name = "Plank", MuscleGroupName = "Abs" },
            new() { Name = "Calf Raises", MuscleGroupName = "Calves" }
        };
}

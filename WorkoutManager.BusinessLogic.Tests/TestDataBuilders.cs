using Bogus;
using WorkoutManager.BusinessLogic.Commands;

namespace WorkoutManager.BusinessLogic.Tests;

public static class TestDataBuilders
{
    public static Faker<CreateExerciseCommand> CreateExerciseCommandFaker => new Faker<CreateExerciseCommand>()
        .RuleFor(c => c.Name, f => f.Lorem.Word())
        .RuleFor(c => c.MuscleGroupId, f => f.Random.Int(1, 10));

    public static Faker<CreateWorkoutPlanCommand> CreateWorkoutPlanCommandFaker => new Faker<CreateWorkoutPlanCommand>()
        .RuleFor(c => c.Name, f => f.Lorem.Word());
}


using Bogus;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.Data.Models;

namespace WorkoutManager.Api.Tests;

public static class TestDataGenerator
{
    public static Faker<CreateExerciseCommand> CreateExerciseCommandFaker(IEnumerable<int> muscleGroupIds)
    {
        return new Faker<CreateExerciseCommand>()
            .RuleFor(x => x.Name, f => f.Commerce.ProductName())
            .RuleFor(x => x.MuscleGroupId, f => f.PickRandom(muscleGroupIds));
    }

    public static Faker<Exercise> ExerciseFaker(Guid userId, IEnumerable<long> muscleGroupIds)
    {
        return new Faker<Exercise>()
            .RuleFor(x => x.Id, f => f.Random.Long(1, 10000))
            .RuleFor(x => x.Name, f => f.Commerce.ProductName())
            .RuleFor(x => x.UserId, userId)
            .RuleFor(x => x.MuscleGroupId, f => f.PickRandom(muscleGroupIds));
    }

    public static Faker<WorkoutPlan> WorkoutPlanFaker(Guid userId)
    {
        return new Faker<WorkoutPlan>()
            .RuleFor(x => x.Id, f => f.Random.Long(1, 10000))
            .RuleFor(x => x.Name, f => f.Lorem.Word())
            .RuleFor(x => x.UserId, userId)
            .RuleFor(x => x.CreatedAt, f => f.Date.Past());
    }

    public static Faker<CreateWorkoutPlanCommand> CreateWorkoutPlanCommandFaker()
    {
        return new Faker<CreateWorkoutPlanCommand>()
            .CustomInstantiator(f =>
            {
                var trainingDays = new Faker<CreateTrainingDayCommand>()
                    .CustomInstantiator(faker =>
                        new CreateTrainingDayCommand(
                            faker.Lorem.Word(),
                            faker.IndexFaker
                        )
                    )
                    .Generate(f.Random.Int(1, 5));

                return new CreateWorkoutPlanCommand(f.Lorem.Word(), trainingDays);
            });
    }
}


using Bogus;
using FluentValidation.TestHelper;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.Validators;
using Xunit;

namespace WorkoutManager.BusinessLogic.Tests.Validators;

public class UpdateWorkoutPlanCommandValidatorTests
{
    private readonly UpdateWorkoutPlanCommandValidator _validator = new();
    private readonly Faker<UpdateWorkoutPlanPayload> _faker;

    public UpdateWorkoutPlanCommandValidatorTests()
    {
        var trainingDayFaker = new Faker<UpdateTrainingDayOrderCommand>()
            .CustomInstantiator(f => new UpdateTrainingDayOrderCommand(f.Random.Int(1, 1000), f.Random.Int(0, 10)));

        _faker = new Faker<UpdateWorkoutPlanPayload>()
            .CustomInstantiator(f => new UpdateWorkoutPlanPayload(
                f.Lorem.Word(),
                trainingDayFaker.Generate(f.Random.Int(1, 5))
            ));
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var model = _faker.Generate();
        var invalidModel = model with { Name = string.Empty };
        var result = _validator.TestValidate(invalidModel);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Too_Long()
    {
        var model = _faker.Generate();
        var invalidModel = model with { Name = new string('a', 201) };
        var result = _validator.TestValidate(invalidModel);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_TrainingDays_Are_Null()
    {
        var model = _faker.Generate();
        var invalidModel = model with { TrainingDays = null! };
        var result = _validator.TestValidate(invalidModel);
        result.ShouldHaveValidationErrorFor(x => x.TrainingDays);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Model_Is_Valid()
    {
        var model = _faker.Generate();
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

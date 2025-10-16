using Bogus;
using FluentValidation.TestHelper;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.Validators;
using Xunit;

namespace WorkoutManager.BusinessLogic.Tests.Validators;

public class AddExerciseToTrainingDayCommandValidatorTests
{
    private readonly AddExerciseToTrainingDayCommandValidator _validator = new();

    private readonly Faker<AddExerciseToTrainingDayCommand> _faker = new Faker<AddExerciseToTrainingDayCommand>()
        .RuleFor(x => x.ExerciseId, f => f.Random.Int(1, 1000))
        .RuleFor(x => x.Order, f => f.Random.Int(0, 10));

    [Fact]
    public void Should_Have_Error_When_ExerciseId_Is_Zero()
    {
        var model = _faker.Generate();
        model.ExerciseId = 0;
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.ExerciseId);
    }

    [Fact]
    public void Should_Have_Error_When_Order_Is_Negative()
    {
        var model = _faker.Generate();
        model.Order = -1;
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Order);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Model_Is_Valid()
    {
        var model = _faker.Generate();
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

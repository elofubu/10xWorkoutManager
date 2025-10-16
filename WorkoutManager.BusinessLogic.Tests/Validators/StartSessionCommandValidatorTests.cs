using Bogus;
using FluentValidation.TestHelper;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.Validators;
using Xunit;

namespace WorkoutManager.BusinessLogic.Tests.Validators;

public class StartSessionCommandValidatorTests
{
    private readonly StartSessionCommandValidator _validator = new();
    private readonly Faker<StartSessionCommand> _faker = new Faker<StartSessionCommand>()
        .RuleFor(x => x.TrainingDayId, f => f.Random.Int(1, 1000));

    [Fact]
    public void Should_Have_Error_When_TrainingDayId_Is_Zero()
    {
        var model = _faker.Generate();
        model.TrainingDayId = 0;
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.TrainingDayId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Model_Is_Valid()
    {
        var model = _faker.Generate();
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

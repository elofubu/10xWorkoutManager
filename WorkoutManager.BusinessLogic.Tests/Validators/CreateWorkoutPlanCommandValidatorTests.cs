using Bogus;
using FluentValidation.TestHelper;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.Validators;
using Xunit;

namespace WorkoutManager.BusinessLogic.Tests.Validators;

public class CreateWorkoutPlanCommandValidatorTests
{
    private readonly CreateWorkoutPlanCommandValidator _validator = new();
    private readonly Faker<CreateWorkoutPlanCommand> _faker;

    public CreateWorkoutPlanCommandValidatorTests()
    {
        var trainingDayFaker = new Faker<CreateTrainingDayCommand>()
            .CustomInstantiator(f => new CreateTrainingDayCommand(f.Lorem.Word(), f.Random.Int(0, 10)));

        _faker = new Faker<CreateWorkoutPlanCommand>()
            .CustomInstantiator(f => new CreateWorkoutPlanCommand(
                f.Lorem.Word(),
                trainingDayFaker.Generate(f.Random.Int(1, 5))
            ));
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var model = new CreateWorkoutPlanCommand("", new List<CreateTrainingDayCommand>());
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Too_Long()
    {
        var model = new CreateWorkoutPlanCommand(new string('a', 201), new List<CreateTrainingDayCommand>());
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_TrainingDays_Are_Empty()
    {
        var model = new CreateWorkoutPlanCommand("Test Plan", new List<CreateTrainingDayCommand>());
        var result = _validator.TestValidate(model);
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

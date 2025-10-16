using Bogus;
using FluentValidation.TestHelper;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Validators;
using Xunit;

namespace WorkoutManager.BusinessLogic.Tests.Validators;

public class CreateExerciseCommandValidatorTests
{
    private readonly CreateExerciseCommandValidator _validator = new();
    private readonly Faker<CreateExerciseDto> _faker = new Faker<CreateExerciseDto>()
        .RuleFor(x => x.Name, f => f.Lorem.Word())
        .RuleFor(x => x.MuscleGroupId, f => f.Random.Int(1, 10));

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var model = _faker.Generate();
        model.Name = string.Empty;
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Too_Long()
    {
        var model = _faker.Generate();
        model.Name = new string('a', 201);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_MuscleGroupId_Is_Zero()
    {
        var model = _faker.Generate();
        model.MuscleGroupId = 0;
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.MuscleGroupId);
    }
    
    [Fact]
    public void Should_Not_Have_Error_When_Model_Is_Valid()
    {
        var model = _faker.Generate();
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

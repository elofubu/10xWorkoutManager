using Bogus;
using FluentValidation.TestHelper;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.Validators;
using Xunit;

namespace WorkoutManager.BusinessLogic.Tests.Validators;

public class UpdateSessionExerciseCommandValidatorTests
{
    private readonly UpdateSessionExerciseCommandValidator _validator = new();
    private readonly Faker<UpdateSessionExerciseCommand> _faker;

    public UpdateSessionExerciseCommandValidatorTests()
    {
        var setFaker = new Faker<UpdateExerciseSetDto>()
            .RuleFor(x => x.Weight, f => f.Random.Decimal(0, 100))
            .RuleFor(x => x.Reps, f => f.Random.Int(1, 12))
            .RuleFor(x => x.IsFailure, f => f.Random.Bool())
            .RuleFor(x => x.Order, f => f.Random.Int(0, 4));

        _faker = new Faker<UpdateSessionExerciseCommand>()
            .RuleFor(x => x.Notes, f => f.Lorem.Sentences(3))
            .RuleFor(x => x.Skipped, f => f.Random.Bool())
            .RuleFor(x => x.Sets, f => setFaker.Generate(f.Random.Int(1, 5)));
    }

    [Fact]
    public void Should_Have_Error_When_Notes_Are_Too_Long()
    {
        var model = _faker.Generate();
        model.Notes = new string('a', 1001);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public void Should_Have_Error_When_Not_Skipped_And_Sets_Are_Empty()
    {
        var model = _faker.Generate();
        model.Skipped = false;
        model.Sets = new List<UpdateExerciseSetDto>();
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Sets);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Skipped_And_Sets_Are_Empty()
    {
        var model = _faker.Generate();
        model.Skipped = true;
        model.Sets = new List<UpdateExerciseSetDto>();
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Sets);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Model_Is_Valid()
    {
        var model = _faker.Generate();
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

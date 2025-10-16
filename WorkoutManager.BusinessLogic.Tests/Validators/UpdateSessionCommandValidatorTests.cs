using Bogus;
using FluentValidation.TestHelper;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.Validators;
using Xunit;

namespace WorkoutManager.BusinessLogic.Tests.Validators;

public class UpdateSessionCommandValidatorTests
{
    private readonly UpdateSessionCommandValidator _validator = new();
    private readonly Faker<UpdateSessionCommand> _faker = new Faker<UpdateSessionCommand>()
        .RuleFor(x => x.Notes, f => f.Lorem.Sentences(5));

    [Fact]
    public void Should_Have_Error_When_Notes_Are_Too_Long()
    {
        var model = _faker.Generate();
        model.Notes = new string('a', 2001);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Notes_Are_Null()
    {
        var model = _faker.Generate();
        model.Notes = null;
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public void Should_Not_Have_Error_When_Model_Is_Valid()
    {
        var model = _faker.Generate();
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

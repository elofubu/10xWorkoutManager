using FluentValidation;
using WorkoutManager.BusinessLogic.Commands;

namespace WorkoutManager.BusinessLogic.Validators;

public class UpdateSessionExerciseCommandValidator : AbstractValidator<UpdateSessionExerciseCommand>
{
    public UpdateSessionExerciseCommandValidator()
    {
        RuleFor(x => x.Notes)
            .MaximumLength(1000).When(x => x.Notes != null)
            .WithMessage("Notes cannot exceed 1000 characters");

        When(x => !x.Skipped, () =>
        {
            RuleFor(x => x.Sets)
                .NotEmpty().WithMessage("At least one set is required for non-skipped exercises");
        });

        RuleForEach(x => x.Sets)
            .SetValidator(new UpdateExerciseSetDtoValidator());
    }
}

public class UpdateExerciseSetDtoValidator : AbstractValidator<UpdateExerciseSetDto>
{
    public UpdateExerciseSetDtoValidator()
    {
        RuleFor(x => x.Weight)
            .GreaterThanOrEqualTo(0).WithMessage("Weight must be non-negative");

        RuleFor(x => x.Reps)
            .GreaterThan(0).WithMessage("Reps must be greater than 0");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be non-negative");
    }
}


using FluentValidation;
using WorkoutManager.BusinessLogic.Commands;

namespace WorkoutManager.BusinessLogic.Validators;

public class UpdateSessionCommandValidator : AbstractValidator<UpdateSessionCommand>
{
    public UpdateSessionCommandValidator()
    {
        RuleFor(x => x.Notes)
            .MaximumLength(2000).When(x => x.Notes != null)
            .WithMessage("Notes cannot exceed 2000 characters");
    }
}


using FluentValidation;
using WorkoutManager.BusinessLogic.Commands;

namespace WorkoutManager.BusinessLogic.Validators;

public class StartSessionCommandValidator : AbstractValidator<StartSessionCommand>
{
    public StartSessionCommandValidator()
    {
        RuleFor(x => x.TrainingDayId)
            .GreaterThan(0).WithMessage("Valid training day must be selected");
    }
}


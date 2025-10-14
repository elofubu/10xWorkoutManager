using FluentValidation;
using WorkoutManager.BusinessLogic.Commands;

namespace WorkoutManager.BusinessLogic.Validators;

public class AddExerciseToTrainingDayCommandValidator : AbstractValidator<AddExerciseToTrainingDayCommand>
{
    public AddExerciseToTrainingDayCommandValidator()
    {
        RuleFor(x => x.ExerciseId)
            .GreaterThan(0).WithMessage("Valid exercise must be selected");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be non-negative");
    }
}


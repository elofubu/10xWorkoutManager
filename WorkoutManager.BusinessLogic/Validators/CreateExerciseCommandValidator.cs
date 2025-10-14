using FluentValidation;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Validators;

public class CreateExerciseCommandValidator : AbstractValidator<CreateExerciseDto>
{
    public CreateExerciseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Exercise name is required")
            .MaximumLength(200).WithMessage("Exercise name cannot exceed 200 characters");

        RuleFor(x => x.MuscleGroupId)
            .GreaterThan(0).WithMessage("Valid muscle group must be selected");
    }
}


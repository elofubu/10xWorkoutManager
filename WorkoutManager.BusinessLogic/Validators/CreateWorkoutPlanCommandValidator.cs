using FluentValidation;
using WorkoutManager.BusinessLogic.Commands;

namespace WorkoutManager.BusinessLogic.Validators;

public class CreateWorkoutPlanCommandValidator : AbstractValidator<CreateWorkoutPlanCommand>
{
    public CreateWorkoutPlanCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Plan name is required")
            .MaximumLength(200).WithMessage("Plan name cannot exceed 200 characters");

        RuleFor(x => x.TrainingDays)
            .NotEmpty().WithMessage("At least one training day is required");

        RuleForEach(x => x.TrainingDays)
            .SetValidator(new CreateTrainingDayCommandValidator());
    }
}

public class CreateTrainingDayCommandValidator : AbstractValidator<CreateTrainingDayCommand>
{
    public CreateTrainingDayCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Training day name is required")
            .MaximumLength(200).WithMessage("Training day name cannot exceed 200 characters");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be non-negative");
    }
}


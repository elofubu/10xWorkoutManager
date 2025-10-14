using FluentValidation;
using WorkoutManager.BusinessLogic.Commands;

namespace WorkoutManager.BusinessLogic.Validators;

public class UpdateWorkoutPlanCommandValidator : AbstractValidator<UpdateWorkoutPlanPayload>
{
    public UpdateWorkoutPlanCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Plan name is required")
            .MaximumLength(200).WithMessage("Plan name cannot exceed 200 characters");

        RuleFor(x => x.TrainingDays)
            .NotNull().WithMessage("Training days list is required");

        RuleForEach(x => x.TrainingDays)
            .SetValidator(new UpdateTrainingDayOrderCommandValidator());
    }
}

public class UpdateTrainingDayOrderCommandValidator : AbstractValidator<UpdateTrainingDayOrderCommand>
{
    public UpdateTrainingDayOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Training day ID must be valid");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be non-negative");
    }
}


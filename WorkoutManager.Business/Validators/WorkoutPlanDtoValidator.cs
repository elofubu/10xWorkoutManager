using FluentValidation;
using WorkoutManager.Business.DTOs;

namespace WorkoutManager.Business.Validators
{
    public class WorkoutPlanDtoValidator : AbstractValidator<WorkoutPlanDto>
    {
        public WorkoutPlanDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Workout plan name is required.")
                .MaximumLength(100).WithMessage("Workout plan name cannot be longer than 100 characters.");
        }
    }
}

using FluentValidation;
using WorkoutManager.Business.DTOs;

namespace WorkoutManager.Business.Validators
{
    public class CreateWorkoutPlanDtoValidator : AbstractValidator<CreateWorkoutPlanDto>
    {
        public CreateWorkoutPlanDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Workout plan name is required.")
                .MaximumLength(100).WithMessage("Workout plan name cannot be longer than 100 characters.");
        }
    }
}

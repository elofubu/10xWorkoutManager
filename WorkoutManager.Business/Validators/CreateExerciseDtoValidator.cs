using FluentValidation;
using WorkoutManager.Business.DTOs;

namespace WorkoutManager.Business.Validators
{
    public class CreateExerciseDtoValidator : AbstractValidator<CreateExerciseDto>
    {
        public CreateExerciseDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Exercise name is required.")
                .MaximumLength(100).WithMessage("Exercise name cannot be longer than 100 characters.");

            RuleFor(x => x.MuscleGroupId)
                .NotEmpty().WithMessage("Muscle group is required.");
        }
    }
}

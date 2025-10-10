using FluentValidation;
using WorkoutManager.Business.DTOs;

namespace WorkoutManager.Business.Validators
{
    public class ExerciseDtoValidator : AbstractValidator<ExerciseDto>
    {
        public ExerciseDtoValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Exercise name is required.")
                .MaximumLength(100).WithMessage("Exercise name cannot be longer than 100 characters.");
            RuleFor(x => x.MuscleGroupId).NotEmpty().WithMessage("Muscle group is required.");
        }
    }
}

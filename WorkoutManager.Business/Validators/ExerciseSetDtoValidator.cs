using FluentValidation;
using WorkoutManager.Business.DTOs;

namespace WorkoutManager.Business.Validators
{
    public class ExerciseSetDtoValidator : AbstractValidator<ExerciseSetDto>
    {
        public ExerciseSetDtoValidator()
        {
            RuleFor(x => x.Weight).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Reps).GreaterThan((short)0);
            RuleFor(x => x.Order).GreaterThan((short)0);
        }
    }
}

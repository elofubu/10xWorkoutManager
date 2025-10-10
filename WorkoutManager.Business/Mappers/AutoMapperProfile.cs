using AutoMapper;
using WorkoutManager.Business.DTOs;
using WorkoutManager.Data.Models;

namespace WorkoutManager.Business.Mappers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // WorkoutPlan mappings
            CreateMap<WorkoutPlan, WorkoutPlanDto>().ReverseMap();
            CreateMap<WorkoutPlan, CreateWorkoutPlanDto>().ReverseMap();
            CreateMap<WorkoutPlan, WorkoutPlanSummaryDto>();

            // TrainingDay and Exercise mappings
            CreateMap<TrainingDay, TrainingDayDto>().ReverseMap();
            CreateMap<PlanDayExercise, PlanDayExerciseDto>()
                .ForMember(dest => dest.ExerciseName, opt => opt.MapFrom(src => src.Exercise.Name));
            CreateMap<Exercise, ExerciseDto>().ReverseMap();
            CreateMap<Exercise, CreateExerciseDto>().ReverseMap();
            CreateMap<MuscleGroup, MuscleGroupDto>().ReverseMap();

            // Session, SessionExercise, and ExerciseSet mappings
            CreateMap<Session, SessionDto>().ReverseMap();
            CreateMap<Session, SessionSummaryDto>()
                .ForMember(dest => dest.ExerciseCount, opt => opt.MapFrom(src => src.Exercises.Count));
            CreateMap<SessionExercise, SessionExerciseDto>()
                .ForMember(dest => dest.ExerciseName, opt => opt.MapFrom(src => src.Exercise.Name))
                .ReverseMap();
            CreateMap<ExerciseSet, ExerciseSetDto>().ReverseMap();
        }
    }
}

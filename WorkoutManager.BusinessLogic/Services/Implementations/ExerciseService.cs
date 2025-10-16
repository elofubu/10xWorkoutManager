using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Exceptions;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class ExerciseService : IExerciseService
{
    private readonly IExerciseRepository _exerciseRepository;

    public ExerciseService(IExerciseRepository exerciseRepository)
    {
        _exerciseRepository = exerciseRepository;
    }

    public async Task<PaginatedList<ExerciseDto>> GetExercisesAsync(
        Guid userId,
        string? search = null,
        int? muscleGroupId = null,
        int page = 1,
        int pageSize = 20)
    {
        var exercises = await _exerciseRepository.GetExercisesForUserAsync(userId);
        
        if (muscleGroupId.HasValue)
        {
            exercises = exercises.Where(e => e.MuscleGroupId == muscleGroupId.Value);
        }
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            exercises = exercises.Where(e => 
                e.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }
        
        var exercisesList = exercises.ToList();

        var dtos = exercisesList.Select(e => new ExerciseDto
        {
            Id = (int)e.Id,
            UserId = e.UserId,
            MuscleGroupId = (int)e.MuscleGroupId,
            Name = e.Name
        }).ToList();

        return new PaginatedList<ExerciseDto>
        {
            Data = dtos.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Pagination = new PaginationInfo
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = exercisesList.Count
            }
        };
    }
    
    public async Task<ExerciseDto?> GetExerciseByIdAsync(int exerciseId)
    {
        var exercise = await _exerciseRepository.GetExerciseByIdAsync(exerciseId);
        if (exercise == null) return null;

        return new ExerciseDto
        {
            Id = (int)exercise.Id,
            UserId = exercise.UserId,
            MuscleGroupId = (int)exercise.MuscleGroupId,
            Name = exercise.Name
        };
    }
    
    public async Task<ExerciseDto> CreateExerciseAsync(CreateExerciseDto dto, Guid userId)
    {
        var existing = await _exerciseRepository.GetExerciseByNameForUserAsync(dto.Name, userId);
        if (existing != null)
        {
            throw new BusinessRuleViolationException($"An exercise named '{dto.Name}' already exists.");
        }

        var exercise = new Exercise
        {
            UserId = userId,
            MuscleGroupId = dto.MuscleGroupId,
            Name = dto.Name
        };

        var created = await _exerciseRepository.CreateExerciseAsync(exercise);

        return new ExerciseDto
        {
            Id = (int)created.Id,
            UserId = created.UserId,
            MuscleGroupId = (int)created.MuscleGroupId,
            Name = created.Name
        };
    }

    public async Task<PreviousExercisePerformanceDto?> GetLastPerformanceAsync(int exerciseId, Guid userId)
    {
        return await _exerciseRepository.GetLastPerformanceAsync(exerciseId, userId);
    }
}


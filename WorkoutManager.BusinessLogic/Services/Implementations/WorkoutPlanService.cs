using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Exceptions;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class WorkoutPlanService : IWorkoutPlanService
{
    private readonly IWorkoutPlanRepository _workoutPlanRepository;

    public WorkoutPlanService(IWorkoutPlanRepository workoutPlanRepository)
    {
        _workoutPlanRepository = workoutPlanRepository;
    }

    public async Task<PaginatedList<WorkoutPlanDto>> GetWorkoutPlansAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var plans = await _workoutPlanRepository.GetWorkoutPlansAsync(userId);
        var planList = plans.ToList();
        var dtos = planList.Select(wp => new WorkoutPlanDto(
            (int)wp.Id,
            wp.Name,
            wp.CreatedAt
        )).ToList();

        return new PaginatedList<WorkoutPlanDto>
        {
            Data = dtos.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            Pagination = new PaginationInfo
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = planList.Count
            }
        };
    }

    public async Task<WorkoutPlanDetailDto> GetWorkoutPlanByIdAsync(long planId, Guid userId)
    {
        var plan = await _workoutPlanRepository.GetWorkoutPlanByIdAsync(planId, userId);
        if (plan == null)
        {
            throw new NotFoundException("WorkoutPlan", planId);
        }

        var isLocked = await _workoutPlanRepository.IsPlanLockedAsync(planId, userId);
        var trainingDaysWithExercises = await _workoutPlanRepository.GetTrainingDaysWithExercisesAsync(planId);
        
        var trainingDays = trainingDaysWithExercises.Select(day => new TrainingDayDto(
            (int)day.Id,
            day.Name,
            day.Order,
            day.PlanDayExercises.Select(pde => new PlanDayExerciseDto(
                (int)pde.Id,
                (int)pde.ExerciseId,
                pde.Exercise?.Name ?? string.Empty,
                pde.Order
            )).ToList()
        )).ToList();

        return new WorkoutPlanDetailDto(
            (int)plan.Id,
            plan.Name,
            isLocked,
            trainingDays
        );
    }

    public async Task<CreatedWorkoutPlanDto> CreateWorkoutPlanAsync(CreateWorkoutPlanCommand command, Guid userId)
    {
        if (string.IsNullOrEmpty(command.Name)) throw new FluentValidation.ValidationException("Name cannot be empty");

        var plan = new WorkoutPlan
        {
            UserId = userId,
            Name = command.Name,
            CreatedAt = DateTime.Now
        };

        var createdPlan = await _workoutPlanRepository.CreateWorkoutPlanAsync(plan, command.TrainingDays);

        var createdTrainingDays = command.TrainingDays.Select((day, index) => new CreatedTrainingDayDto(
            index + 1, // Placeholder ID
            day.Name,
            day.Order
        )).ToList();

        return new CreatedWorkoutPlanDto(
            createdPlan.Id,
            createdPlan.Name,
            createdTrainingDays
        );
    }

    public async Task UpdateWorkoutPlanAsync(long planId, UpdateWorkoutPlanPayload payload, Guid userId)
    {
        var plan = await _workoutPlanRepository.GetWorkoutPlanByIdAsync(planId, userId);
        if (plan == null)
        {
            throw new NotFoundException("WorkoutPlan", planId);
        }

        if (await _workoutPlanRepository.IsPlanLockedAsync(planId, userId))
        {
            throw new BusinessRuleViolationException("Cannot update a workout plan that is currently being used in an active session.");
        }

        plan.Name = payload.Name;
        await _workoutPlanRepository.UpdateWorkoutPlanAsync(plan, payload.TrainingDays);
    }

    public async Task DeleteWorkoutPlanAsync(long planId, Guid userId)
    {
        var plan = await _workoutPlanRepository.GetWorkoutPlanByIdAsync(planId, userId);
        if (plan == null)
        {
            throw new NotFoundException("WorkoutPlan", planId);
        }

        if (await _workoutPlanRepository.IsPlanLockedAsync(planId, userId))
        {
            throw new BusinessRuleViolationException("Cannot delete a workout plan that is currently being used in an active session.");
        }
        
        await _workoutPlanRepository.DeleteWorkoutPlanAsync(planId);
    }

    public async Task<bool> IsPlanLockedAsync(long planId, Guid userId)
    {
        return await _workoutPlanRepository.IsPlanLockedAsync(planId, userId);
    }
}


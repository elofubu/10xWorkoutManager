using Supabase;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Exceptions;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class WorkoutPlanService : IWorkoutPlanService
{
    private readonly Client _supabaseClient;

    public WorkoutPlanService(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<PaginatedList<WorkoutPlanDto>> GetWorkoutPlansAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var from = (page - 1) * pageSize;
        var to = from + pageSize - 1;

        var response = await _supabaseClient
            .From<WorkoutPlan>()
            .Where(wp => wp.UserId == userId)
            .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
            .Range(from, to)
            .Get();

        var dtos = response.Models.Select(wp => new WorkoutPlanDto(
            (int)wp.Id,
            wp.Name,
            wp.CreatedAt
        )).ToList();

        return new PaginatedList<WorkoutPlanDto>
        {
            Data = dtos,
            Pagination = new PaginationInfo
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = response.Models.Count
            }
        };
    }

    public async Task<WorkoutPlanDetailDto> GetWorkoutPlanByIdAsync(int planId, Guid userId)
    {
        // Get the workout plan
        var plan = await _supabaseClient
            .From<WorkoutPlan>()
            .Where(wp => wp.Id == planId)
            .Where(wp => wp.UserId == userId)
            .Single();

        if (plan == null)
        {
            throw new NotFoundException("WorkoutPlan", planId);
        }

        // Check if plan is locked (has an active session)
        var isLocked = await IsPlanLockedAsync(planId, userId);

        // Get training days for this plan
        var trainingDaysResponse = await _supabaseClient
            .From<TrainingDay>()
            .Where(td => td.PlanId == planId)
            .Order("order", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();

        var trainingDays = new List<TrainingDayDto>();

        foreach (var day in trainingDaysResponse.Models)
        {
            // Get exercises for each training day
            var planDayExercisesResponse = await _supabaseClient
                .From<PlanDayExercise>()
                .Where(pde => pde.TrainingDayId == day.Id)
                .Order("order", Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            var exercises = new List<PlanDayExerciseDto>();

            foreach (var pde in planDayExercisesResponse.Models)
            {
                // Get exercise details
                var exercise = await _supabaseClient
                    .From<Exercise>()
                    .Where(e => e.Id == pde.ExerciseId)
                    .Single();

                if (exercise != null)
                {
                    exercises.Add(new PlanDayExerciseDto(
                        (int)pde.Id,
                        (int)pde.ExerciseId,
                        exercise.Name,
                        pde.Order
                    ));
                }
            }

            trainingDays.Add(new TrainingDayDto(
                (int)day.Id,
                day.Name,
                day.Order,
                exercises
            ));
        }

        return new WorkoutPlanDetailDto(
            (int)plan.Id,
            plan.Name,
            isLocked,
            trainingDays
        );
    }

    public async Task<CreatedWorkoutPlanDto> CreateWorkoutPlanAsync(CreateWorkoutPlanCommand command, Guid userId)
    {
        // Create the workout plan
        var plan = new WorkoutPlan
        {
            UserId = userId,
            Name = command.Name,
            CreatedAt = DateTime.Now
        };

        var planResponse = await _supabaseClient
            .From<WorkoutPlan>()
            .Insert(plan);

        var createdPlan = planResponse.Models.First();

        // Create training days
        var createdTrainingDays = new List<CreatedTrainingDayDto>();

        foreach (var dayCommand in command.TrainingDays)
        {
            var trainingDay = new TrainingDay
            {
                PlanId = createdPlan.Id,
                Name = dayCommand.Name,
                Order = (short)dayCommand.Order
            };

            var dayResponse = await _supabaseClient
                .From<TrainingDay>()
                .Insert(trainingDay);

            var createdDay = dayResponse.Models.First();

            createdTrainingDays.Add(new CreatedTrainingDayDto(
                (int)createdDay.Id,
                createdDay.Name,
                createdDay.Order
            ));
        }

        return new CreatedWorkoutPlanDto(
            (int)createdPlan.Id,
            createdPlan.Name,
            createdTrainingDays
        );
    }

    public async Task UpdateWorkoutPlanAsync(int planId, UpdateWorkoutPlanPayload payload, Guid userId)
    {
        // Check if plan exists and belongs to user
        var plan = await _supabaseClient
            .From<WorkoutPlan>()
            .Where(wp => wp.Id == planId)
            .Where(wp => wp.UserId == userId)
            .Single();

        if (plan == null)
        {
            throw new NotFoundException("WorkoutPlan", planId);
        }

        // Check if plan is locked
        if (await IsPlanLockedAsync(planId, userId))
        {
            throw new BusinessRuleViolationException("Cannot update a workout plan that is currently being used in an active session.");
        }

        // Update plan name
        plan.Name = payload.Name;
        await _supabaseClient
            .From<WorkoutPlan>()
            .Update(plan);

        // Update training day orders
        foreach (var dayUpdate in payload.TrainingDays)
        {
            var day = await _supabaseClient
                .From<TrainingDay>()
                .Where(td => td.Id == dayUpdate.Id)
                .Where(td => td.PlanId == planId)
                .Single();

            if (day != null)
            {
                day.Order = (short)dayUpdate.Order;
                await _supabaseClient
                    .From<TrainingDay>()
                    .Update(day);
            }
        }
    }

    public async Task DeleteWorkoutPlanAsync(int planId, Guid userId)
    {
        // Check if plan exists and belongs to user
        var plan = await _supabaseClient
            .From<WorkoutPlan>()
            .Where(wp => wp.Id == planId)
            .Where(wp => wp.UserId == userId)
            .Single();

        if (plan == null)
        {
            throw new NotFoundException("WorkoutPlan", planId);
        }

        // Check if plan is locked
        if (await IsPlanLockedAsync(planId, userId))
        {
            throw new BusinessRuleViolationException("Cannot delete a workout plan that is currently being used in an active session.");
        }

        // Delete the plan (cascade will handle training days and plan day exercises)
        await _supabaseClient
            .From<WorkoutPlan>()
            .Where(wp => wp.Id == planId)
            .Delete();
    }

    public async Task<bool> IsPlanLockedAsync(int planId, Guid userId)
    {
        // Check if there's an active session (end_time is null) for this plan
        var activeSessionsResponse = await _supabaseClient
            .From<Session>()
            .Filter("plan_id", Supabase.Postgrest.Constants.Operator.Equals, planId)
            .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
            .Filter<DateTime?>("end_time", Supabase.Postgrest.Constants.Operator.Is, null)
            .Get();

        return activeSessionsResponse.Models.Any();
    }
}


using Supabase;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Exceptions;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class SessionService : ISessionService
{
    private readonly Client _supabaseClient;

    public SessionService(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<SessionDetailsDto> StartSessionAsync(int trainingDayId, Guid userId)
    {
        // Check if user already has an active session
        if (await HasActiveSessionAsync(userId))
        {
            throw new BusinessRuleViolationException("You already have an active session. Please finish it before starting a new one.");
        }

        // Get the training day and verify it exists
        var trainingDay = await _supabaseClient
            .From<TrainingDay>()
            .Where(td => td.Id == trainingDayId)
            .Single();

        if (trainingDay == null)
        {
            throw new NotFoundException("TrainingDay", trainingDayId);
        }

        // Verify the plan belongs to the user
        var plan = await _supabaseClient
            .From<WorkoutPlan>()
            .Where(wp => wp.Id == trainingDay.PlanId)
            .Where(wp => wp.UserId == userId)
            .Single();

        if (plan == null)
        {
            throw new NotFoundException("WorkoutPlan", trainingDay.PlanId);
        }

        // Create the session
        var session = new Session
        {
            UserId = userId,
            PlanId = trainingDay.PlanId,
            StartTime = DateTime.UtcNow,
            EndTime = null
        };

        var sessionResponse = await _supabaseClient
            .From<Session>()
            .Insert(session);

        var createdSession = sessionResponse.Models.First();

        // Get exercises for this training day
        var planDayExercisesResponse = await _supabaseClient
            .From<PlanDayExercise>()
            .Where(pde => pde.TrainingDayId == trainingDayId)
            .Order("order", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();

        // Create session exercises from plan day exercises
        var sessionExercises = new List<SessionExerciseDetailsDto>();

        foreach (var pde in planDayExercisesResponse.Models)
        {
            var sessionExercise = new SessionExercise
            {
                SessionId = createdSession.Id,
                ExerciseId = pde.ExerciseId,
                Order = pde.Order,
                Skipped = false
            };

            var seResponse = await _supabaseClient
                .From<SessionExercise>()
                .Insert(sessionExercise);

            var createdSE = seResponse.Models.First();

            sessionExercises.Add(new SessionExerciseDetailsDto
            {
                Id = (int)createdSE.Id,
                ExerciseId = (int)createdSE.ExerciseId,
                Notes = createdSE.Notes,
                Skipped = createdSE.Skipped,
                Order = createdSE.Order,
                Sets = new List<ExerciseSetDto>()
            });
        }

        return new SessionDetailsDto
        {
            Id = (int)createdSession.Id,
            Notes = createdSession.Notes,
            StartTime = createdSession.StartTime,
            EndTime = createdSession.EndTime,
            Exercises = sessionExercises
        };
    }

    public async Task<PaginatedList<SessionSummaryDto>> GetSessionHistoryAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var from = (page - 1) * pageSize;
        var to = from + pageSize - 1;

        var response = await _supabaseClient
            .From<Session>()
            .Where(s => s.UserId == userId)
            .Order("start_time", Supabase.Postgrest.Constants.Ordering.Descending)
            .Range(from, to)
            .Get();

        var summaries = new List<SessionSummaryDto>();

        foreach (var session in response.Models)
        {
            string? planName = null;
            string? trainingDayName = null;

            // Get plan name if available
            if (session.PlanId.HasValue)
            {
                var plan = await _supabaseClient
                    .From<WorkoutPlan>()
                    .Where(wp => wp.Id == session.PlanId.Value)
                    .Single();

                if (plan != null)
                {
                    planName = plan.Name;
                }
            }

            summaries.Add(new SessionSummaryDto
            {
                Id = (int)session.Id,
                PlanId = session.PlanId.HasValue ? (int)session.PlanId.Value : 0,
                PlanName = planName,
                TrainingDayName = trainingDayName,
                Notes = session.Notes,
                StartTime = session.StartTime,
                EndTime = session.EndTime
            });
        }

        return new PaginatedList<SessionSummaryDto>
        {
            Data = summaries,
            Pagination = new PaginationInfo
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = response.Models.Count
            }
        };
    }

    public async Task<SessionDetailsDto> GetSessionByIdAsync(int sessionId, Guid userId)
    {
        var session = await _supabaseClient
            .From<Session>()
            .Where(s => s.Id == sessionId)
            .Where(s => s.UserId == userId)
            .Single();

        if (session == null)
        {
            throw new NotFoundException("Session", sessionId);
        }

        // Get session exercises
        var sessionExercisesResponse = await _supabaseClient
            .From<SessionExercise>()
            .Where(se => se.SessionId == sessionId)
            .Order("order", Supabase.Postgrest.Constants.Ordering.Ascending)
            .Get();

        var exercises = new List<SessionExerciseDetailsDto>();

        foreach (var se in sessionExercisesResponse.Models)
        {
            // Get sets for this session exercise
            var setsResponse = await _supabaseClient
                .From<ExerciseSet>()
                .Where(es => es.SessionExerciseId == se.Id)
                .Order("order", Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            var sets = setsResponse.Models.Select(es => new ExerciseSetDto
            {
                Id = (int)es.Id,
                Weight = es.Weight,
                Reps = es.Reps,
                IsFailure = es.IsFailure,
                Order = es.Order
            }).ToList();

            exercises.Add(new SessionExerciseDetailsDto
            {
                Id = (int)se.Id,
                ExerciseId = (int)se.ExerciseId,
                Notes = se.Notes,
                Skipped = se.Skipped,
                Order = se.Order,
                Sets = sets
            });
        }

        return new SessionDetailsDto
        {
            Id = (int)session.Id,
            Notes = session.Notes,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            Exercises = exercises
        };
    }

    public async Task UpdateSessionNotesAsync(int sessionId, string? notes, Guid userId)
    {
        var session = await _supabaseClient
            .From<Session>()
            .Where(s => s.Id == sessionId)
            .Where(s => s.UserId == userId)
            .Single();

        if (session == null)
        {
            throw new NotFoundException("Session", sessionId);
        }

        session.Notes = notes;
        await _supabaseClient
            .From<Session>()
            .Update(session);
    }

    public async Task FinishSessionAsync(int sessionId, string? notes, Guid userId)
    {
        var session = await _supabaseClient
            .From<Session>()
            .Where(s => s.Id == sessionId)
            .Where(s => s.UserId == userId)
            .Single();

        if (session == null)
        {
            throw new NotFoundException("Session", sessionId);
        }

        if (session.EndTime.HasValue)
        {
            throw new BusinessRuleViolationException("This session has already been finished.");
        }

        session.Notes = notes;
        session.EndTime = DateTime.UtcNow;
        
        await _supabaseClient
            .From<Session>()
            .Update(session);
    }

    public async Task<bool> HasActiveSessionAsync(Guid userId)
    {
        var activeSessionsResponse = await _supabaseClient
            .From<Session>()
            .Where(s => s.UserId == userId)
            .Where(s => s.EndTime == null)
            .Get();

        return activeSessionsResponse.Models.Any();
    }
}


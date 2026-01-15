using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Exceptions;
using WorkoutManager.BusinessLogic.Services.Interfaces;
using WorkoutManager.Data.Models;

namespace WorkoutManager.BusinessLogic.Services.Implementations;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;

    public SessionService(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<SessionDetailsDto> StartSessionAsync(long trainingDayId, Guid userId)
    {
        if (await _sessionRepository.HasActiveSessionAsync(userId))
        {
            throw new BusinessRuleViolationException("You already have an active session. Please finish it before starting a new one.");
        }

        var trainingDay = await _sessionRepository.GetTrainingDayByIdAsync(trainingDayId);
        if (trainingDay == null)
        {
            throw new NotFoundException("TrainingDay", trainingDayId);
        }

        var plan = await _sessionRepository.GetWorkoutPlanByIdAsync(trainingDay.PlanId, userId);
        if (plan == null)
        {
            throw new NotFoundException("WorkoutPlan", trainingDay.PlanId);
        }

        var session = new Session
        {
            UserId = userId,
            PlanId = trainingDay.PlanId,
            TrainingDayId = trainingDayId,
            StartTime = DateTime.UtcNow
        };
        var createdSession = await _sessionRepository.CreateSessionAsync(session);

        // Pre-populate exercises from the training day
        var planDayExercises = await _sessionRepository.GetPlanDayExercisesAsync(trainingDayId);
        var sessionExercises = new List<SessionExerciseDetailsDto>();

        foreach (var pde in planDayExercises)
        {
            var sessionExercise = new SessionExercise
            {
                SessionId = createdSession.Id,
                ExerciseId = pde.ExerciseId,
                Order = pde.Order
            };
            var createdSE = await _sessionRepository.CreateSessionExerciseAsync(sessionExercise);

            sessionExercises.Add(new SessionExerciseDetailsDto
            {
                Id = (int)createdSE.Id,
                ExerciseId = (int)createdSE.ExerciseId,
                Order = createdSE.Order,
                Sets = new List<ExerciseSetDto>()
            });
        }

        return new SessionDetailsDto
        {
            Id = (int)createdSession.Id,
            PlanId = createdSession.PlanId,
            TrainingDayId = createdSession.TrainingDayId,
            PlanName = plan.Name,
            TrainingDayName = trainingDay.Name,
            StartTime = createdSession.StartTime,
            Exercises = sessionExercises
        };
    }

    public async Task<PaginatedList<SessionSummaryDto>> GetSessionHistoryAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        var sessions = await _sessionRepository.GetSessionHistoryAsync(userId, page, pageSize);
        var sessionList = sessions.ToList();
        var summaries = sessionList.Where(s => s.EndTime.HasValue).Select(s => new SessionSummaryDto
        {
            Id = (int)s.Id,
            PlanId = s.PlanId,
            TrainingDayId = s.TrainingDayId,
            PlanName = s.Plan?.Name,
            TrainingDayName = s.TrainingDay?.Name,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            Notes = s.Notes,
            Exercises = s.SessionExercises.Select(se => new SessionExerciseDetailsDto
            {
                Id = (int)se.Id,
                ExerciseId = (int)se.ExerciseId,
                Notes = se.Notes,
                Skipped = se.Skipped,
                Order = se.Order,
                ExerciseName = se.Exercise?.Name,
                Sets = se.Sets.Select(es => new ExerciseSetDto
                {
                    Id = (int)es.Id,
                    Weight = es.Weight,
                    Reps = es.Reps,
                    IsFailure = es.IsFailure,
                    Order = es.Order
                }).ToList()
            }).ToList()
        }).ToList();

        return new PaginatedList<SessionSummaryDto>
        {
            Data = summaries,
            Pagination = new PaginationInfo { Page = page, PageSize = pageSize, TotalCount = sessionList.Count }
        };
    }

    public async Task<SessionDetailsDto> GetSessionByIdAsync(long sessionId, Guid userId)
    {
        var session = await _sessionRepository.GetSessionByIdAsync(sessionId, userId);
        if (session == null)
        {
            throw new NotFoundException("Session", sessionId);
        }

        var sessionExercises = await _sessionRepository.GetSessionExercisesWithSetsAsync(sessionId);
        var exercises = sessionExercises.Select(se => new SessionExerciseDetailsDto
        {
            Id = (int)se.Id,
            ExerciseId = (int)se.ExerciseId,
            Notes = se.Notes,
            Skipped = se.Skipped,
            Order = se.Order,
            ExerciseName = se.Exercise?.Name,
            Sets = se.Sets.Select(es => new ExerciseSetDto
            {
                Id = (int)es.Id,
                Weight = es.Weight,
                Reps = es.Reps,
                IsFailure = es.IsFailure,
                Order = es.Order
            }).ToList()
        }).ToList();

        return new SessionDetailsDto
        {
            Id = (int)session.Id,
            PlanId = session.PlanId,
            TrainingDayId = session.TrainingDayId,
            PlanName = session.Plan?.Name,
            TrainingDayName = session.TrainingDay?.Name,
            Notes = session.Notes,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            Exercises = exercises
        };
    }

    public async Task UpdateSessionNotesAsync(long sessionId, string? notes, Guid userId)
    {
        var session = await _sessionRepository.GetSessionByIdAsync(sessionId, userId);
        if (session == null)
        {
            throw new NotFoundException("Session", sessionId);
        }

        session.Notes = notes;
        await _sessionRepository.UpdateSessionAsync(session);
    }

    public async Task FinishSessionAsync(long sessionId, string? notes, Guid userId)
    {
        var session = await _sessionRepository.GetSessionByIdAsync(sessionId, userId);
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
        await _sessionRepository.UpdateSessionAsync(session);
    }

    public async Task<bool> HasActiveSessionAsync(Guid userId)
    {
        return await _sessionRepository.HasActiveSessionAsync(userId);
    }

    public async Task<SessionDetailsDto?> GetActiveSessionAsync(Guid userId)
    {
        var activeSession = await _sessionRepository.GetActiveSessionAsync(userId);
        if (activeSession == null)
        {
            return null;
        }

        return await GetSessionByIdAsync((int)activeSession.Id, userId);
    }
}


using WorkoutManager.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.BusinessLogic.Services.Interfaces;

public interface ISessionRepository
{
    Task<bool> HasActiveSessionAsync(Guid userId);
    Task<TrainingDay?> GetTrainingDayByIdAsync(long trainingDayId);
    Task<WorkoutPlan?> GetWorkoutPlanByIdAsync(long planId, Guid userId);
    Task<Session> CreateSessionAsync(Session session);
    Task<IEnumerable<PlanDayExercise>> GetPlanDayExercisesAsync(long trainingDayId);
    Task<SessionExercise> CreateSessionExerciseAsync(SessionExercise sessionExercise);
    Task<IEnumerable<Session>> GetSessionHistoryAsync(Guid userId, int page, int pageSize);
    Task<Session?> GetSessionByIdAsync(long sessionId, Guid userId);
    Task<IEnumerable<SessionExercise>> GetSessionExercisesWithSetsAsync(long sessionId);
    Task UpdateSessionAsync(Session session);
    Task<Session?> GetActiveSessionAsync(Guid userId);
}

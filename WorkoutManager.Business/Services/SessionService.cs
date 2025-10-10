using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using WorkoutManager.Business.DTOs;
using WorkoutManager.Business.Interfaces;
using WorkoutManager.Business.Exceptions;
using WorkoutManager.Data.Models;
using Postgrest.Exceptions;

namespace WorkoutManager.Business.Services
{
    public class SessionService : ISessionService
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly IMapper _mapper;
        private readonly ILogger<SessionService> _logger;
        private readonly IValidator<ExerciseSetDto> _exerciseSetValidator;

        public SessionService(Supabase.Client supabaseClient, IMapper mapper, ILogger<SessionService> logger, IValidator<ExerciseSetDto> exerciseSetValidator)
        {
            _supabaseClient = supabaseClient;
            _mapper = mapper;
            _logger = logger;
            _exerciseSetValidator = exerciseSetValidator;
        }

        public async Task AddExerciseToSessionAsync(long sessionId, long exerciseId)
        {
            try
            {
                var session = await _supabaseClient.From<Session>().Filter("id", Postgrest.Constants.Operator.Equals, sessionId).Single();
                if (session == null)
                {
                    throw new NotFoundException($"Session with id {sessionId} not found.");
                }

                var exercise = await _supabaseClient.From<Exercise>().Filter("id", Postgrest.Constants.Operator.Equals, exerciseId).Single();
                if (exercise == null)
                {
                    throw new NotFoundException($"Exercise with id {exerciseId} not found.");
                }

                var maxOrder = await _supabaseClient.From<SessionExercise>()
                                       .Where(se => se.SessionId == sessionId)
                                       .Get();

                var newSessionExercise = new SessionExercise
                {
                    SessionId = sessionId,
                    ExerciseId = exerciseId,
                    Order = (short)(maxOrder.Models.Count > 0 ? maxOrder.Models.Max(se => se.Order) + 1 : 1)
                };

                await _supabaseClient.From<SessionExercise>().Insert(newSessionExercise);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to add exercise {exerciseId} to session {sessionId}.", exerciseId, sessionId);
                throw new DatabaseException("An error occurred while adding the exercise to the session.", ex);
            }
        }

        public async Task AddSetToSessionExerciseAsync(long sessionExerciseId, ExerciseSetDto setDto)
        {
            var validationResult = await _exerciseSetValidator.ValidateAsync(setDto);
            if (!validationResult.IsValid)
            {
                throw new Exceptions.ValidationException(validationResult.Errors);
            }
            try
            {
                var sessionExercise = await _supabaseClient.From<SessionExercise>().Filter("id", Postgrest.Constants.Operator.Equals, sessionExerciseId).Single();
                if (sessionExercise == null)
                {
                    throw new NotFoundException($"Session exercise with id {sessionExerciseId} not found.");
                }

                var newSet = _mapper.Map<ExerciseSet>(setDto);
                newSet.SessionExerciseId = sessionExerciseId;

                await _supabaseClient.From<ExerciseSet>().Insert(newSet);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to add set to session exercise {sessionExerciseId}.", sessionExerciseId);
                throw new DatabaseException("An error occurred while adding the set to the session exercise.", ex);
            }
        }

        public async Task EndSessionAsync(long sessionId)
        {
            try
            {
                var session = await _supabaseClient.From<Session>().Filter("id", Postgrest.Constants.Operator.Equals, sessionId).Single();
                if (session == null)
                {
                    throw new NotFoundException($"Session with id {sessionId} not found.");
                }

                session.EndTime = DateTime.UtcNow;
                await _supabaseClient.From<Session>().Update(session);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to end session with id {sessionId}.", sessionId);
                throw new DatabaseException($"An error occurred while ending the session with id {sessionId}.", ex);
            }
        }

        public async Task<SessionDto> GetSessionByIdAsync(long id)
        {
            try
            {
                var response = await _supabaseClient.From<Session>()
                    .Where(s => s.Id == id)
                    .Get();
                
                var session = response.Models.FirstOrDefault();

                if (session == null)
                {
                    throw new NotFoundException($"Session with id {id} not found.");
                }

                return _mapper.Map<SessionDto>(session);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to retrieve session with id {id}.", id);
                throw new DatabaseException($"An error occurred while retrieving the session with id {id}.", ex);
            }
        }

        public async Task<SessionDto> GetLatestSessionAsync()
        {
            try
            {
                var response = await _supabaseClient.From<Session>()
                    .Order("start_time", Postgrest.Constants.Ordering.Descending)
                    .Limit(1)
                    .Get();

                var latestSession = response.Models.FirstOrDefault();

                if (latestSession == null)
                {
                    return null!;
                }

                return await GetSessionByIdAsync(latestSession.Id);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to retrieve the latest session.");
                throw new DatabaseException("An error occurred while retrieving the latest session.", ex);
            }
        }

        public async Task<IEnumerable<SessionSummaryDto>> GetSessionHistoryAsync()
        {
            try
            {
                var response = await _supabaseClient.From<Session>().Get();
                var sessions = response.Models;
                return _mapper.Map<IEnumerable<SessionSummaryDto>>(sessions);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to retrieve session history.");
                throw new DatabaseException("An error occurred while retrieving the session history.", ex);
            }
        }

        public async Task RemoveExerciseFromSessionAsync(long sessionId, long sessionExerciseId)
        {
            try
            {
                var sessionExercise = await _supabaseClient.From<SessionExercise>().Filter("id", Postgrest.Constants.Operator.Equals, sessionExerciseId).Single();
                if (sessionExercise == null || sessionExercise.SessionId != sessionId)
                {
                    throw new NotFoundException($"Exercise with id {sessionExerciseId} not found in session {sessionId}.");
                }

                await _supabaseClient.From<SessionExercise>().Delete(sessionExercise);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to remove exercise {sessionExerciseId} from session {sessionId}.", sessionExerciseId, sessionId);
                throw new DatabaseException("An error occurred while removing the exercise from the session.", ex);
            }
        }

        public async Task RemoveSetFromSessionExerciseAsync(long sessionExerciseId, long setId)
        {
            try
            {
                var exerciseSet = await _supabaseClient.From<ExerciseSet>().Filter("id", Postgrest.Constants.Operator.Equals, setId).Single();
                if (exerciseSet == null || exerciseSet.SessionExerciseId != sessionExerciseId)
                {
                    throw new NotFoundException($"Set with id {setId} not found in session exercise {sessionExerciseId}.");
                }

                await _supabaseClient.From<ExerciseSet>().Delete(exerciseSet);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to remove set {setId} from session exercise {sessionExerciseId}.", setId, sessionExerciseId);
                throw new DatabaseException("An error occurred while removing the set from the session exercise.", ex);
            }
        }

        public async Task SkipExerciseAsync(long sessionExerciseId)
        {
            try
            {
                var sessionExercise = await _supabaseClient.From<SessionExercise>().Filter("id", Postgrest.Constants.Operator.Equals, sessionExerciseId).Single();
                if (sessionExercise == null)
                {
                    throw new NotFoundException($"Session exercise with id {sessionExerciseId} not found.");
                }

                sessionExercise.Skipped = true;
                await _supabaseClient.From<SessionExercise>().Update(sessionExercise);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to skip session exercise with id {sessionExerciseId}.", sessionExerciseId);
                throw new DatabaseException($"An error occurred while skipping the session exercise with id {sessionExerciseId}.", ex);
            }
        }

        public async Task<SessionDto> StartSessionAsync(long? planId)
        {
            try
            {
                var newSession = new Session
                {
                    PlanId = planId,
                    StartTime = DateTime.UtcNow
                };

                var sessionResponse = await _supabaseClient.From<Session>().Insert(newSession);
                var createdSession = sessionResponse.Models.First();

                if (planId.HasValue)
                {
                    var trainingDayResponse = await _supabaseClient.From<TrainingDay>()
                        .Where(td => td.PlanId == planId.Value)
                        .Order("order", Postgrest.Constants.Ordering.Ascending)
                        .Limit(1)
                        .Get();
                    
                    var firstTrainingDay = trainingDayResponse.Models.FirstOrDefault();

                    if (firstTrainingDay != null)
                    {
                        var planDayExercisesResponse = await _supabaseClient.From<PlanDayExercise>()
                            .Where(pde => pde.TrainingDayId == firstTrainingDay.Id)
                            .Get();
                        
                        var sessionExercises = planDayExercisesResponse.Models.Select(pde => new SessionExercise
                        {
                            SessionId = createdSession.Id,
                            ExerciseId = pde.ExerciseId,
                            Order = pde.Order
                        }).ToList();

                        if (sessionExercises.Any())
                        {
                            await _supabaseClient.From<SessionExercise>().Insert(sessionExercises);
                        }
                    }
                }

                return await GetSessionByIdAsync(createdSession.Id);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to start session.");
                throw new DatabaseException("An error occurred while starting the session.", ex);
            }
        }

        public async Task UpdateSetAsync(long setId, ExerciseSetDto setDto)
        {
            if (setId != setDto.Id)
            {
                throw new ArgumentException("ID mismatch");
            }

            var validationResult = await _exerciseSetValidator.ValidateAsync(setDto);
            if (!validationResult.IsValid)
            {
                throw new Exceptions.ValidationException(validationResult.Errors);
            }

            try
            {
                var existingSet = await _supabaseClient.From<ExerciseSet>().Filter("id", Postgrest.Constants.Operator.Equals, setId).Single();
                if (existingSet == null)
                {
                    throw new NotFoundException($"Set with id {setId} not found.");
                }

                _mapper.Map(setDto, existingSet);

                await _supabaseClient.From<ExerciseSet>().Update(existingSet);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to update set with id {setId}.", setId);
                throw new DatabaseException($"An error occurred while updating the set with id {setId}.", ex);
            }
        }
    }
}

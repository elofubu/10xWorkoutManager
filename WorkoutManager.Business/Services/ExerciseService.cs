using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using WorkoutManager.Business.DTOs;
using WorkoutManager.Business.Exceptions;
using WorkoutManager.Business.Interfaces;
using WorkoutManager.Data.Models;
using Postgrest.Exceptions;

namespace WorkoutManager.Business.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly IMapper _mapper;
        private readonly ILogger<ExerciseService> _logger;
        private readonly IValidator<CreateExerciseDto> _createExerciseValidator;
        private readonly IValidator<ExerciseDto> _updateExerciseValidator;

        public ExerciseService(Supabase.Client supabaseClient, IMapper mapper, ILogger<ExerciseService> logger, IValidator<CreateExerciseDto> createExerciseValidator, IValidator<ExerciseDto> updateExerciseValidator)
        {
            _supabaseClient = supabaseClient;
            _mapper = mapper;
            _logger = logger;
            _createExerciseValidator = createExerciseValidator;
            _updateExerciseValidator = updateExerciseValidator;
        }

        public async Task<ExerciseDto> CreateExerciseAsync(CreateExerciseDto exerciseDto)
        {
            var validationResult = await _createExerciseValidator.ValidateAsync(exerciseDto);
            if (!validationResult.IsValid)
            {
                throw new Exceptions.ValidationException(validationResult.Errors);
            }

            try
            {
                var exercise = _mapper.Map<Exercise>(exerciseDto);
                var response = await _supabaseClient.From<Exercise>().Insert(exercise);
                var createdExercise = response.Models.First();
                return _mapper.Map<ExerciseDto>(createdExercise);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to create exercise.");
                if (ex.Message.Contains("duplicate key value violates unique constraint"))
                {
                    throw new DuplicateEntryException("An exercise with this name already exists.", ex);
                }
                throw new DatabaseException("An error occurred while creating the exercise.", ex);
            }
        }

        public async Task DeleteExerciseAsync(long id)
        {
            try
            {
                var existingExercise = await _supabaseClient.From<Exercise>().Filter("id", Postgrest.Constants.Operator.Equals, id).Single();
                if (existingExercise == null)
                {
                    throw new NotFoundException($"Exercise with id {id} not found.");
                }

                await _supabaseClient.From<Exercise>().Delete(existingExercise);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to delete exercise with id {id}.", id);
                throw new DatabaseException($"An error occurred while deleting the exercise with id {id}.", ex);
            }
        }

        public async Task<IEnumerable<ExerciseDto>> GetAllExercisesAsync()
        {
            try
            {
                var response = await _supabaseClient.From<Exercise>().Get();
                var exercises = response.Models;
                return _mapper.Map<IEnumerable<ExerciseDto>>(exercises);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to retrieve exercises.");
                throw new DatabaseException("An error occurred while retrieving exercises.", ex);
            }
        }

        public async Task<IEnumerable<MuscleGroupDto>> GetAllMuscleGroupsAsync()
        {
            try
            {
                var response = await _supabaseClient.From<MuscleGroup>().Get();
                var muscleGroups = response.Models;

                return _mapper.Map<IEnumerable<MuscleGroupDto>>(muscleGroups);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to retrieve muscle groups.");
                throw new DatabaseException("An error occurred while retrieving the muscle groups.", ex);
            }
        }

        public async Task<ExerciseDto> GetExerciseByIdAsync(long id)
        {
            try
            {
                var response = await _supabaseClient.From<Exercise>().Filter("id", Postgrest.Constants.Operator.Equals, id).Single();
                if (response == null)
                {
                    throw new NotFoundException($"Exercise with id {id} not found.");
                }
                return _mapper.Map<ExerciseDto>(response);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to retrieve exercise with id {id}.", id);
                throw new DatabaseException($"An error occurred while retrieving the exercise with id {id}.", ex);
            }
        }

        public async Task UpdateExerciseAsync(long id, ExerciseDto exerciseDto)
        {
            if (id != exerciseDto.Id)
            {
                throw new ArgumentException("ID mismatch");
            }

            var validationResult = await _updateExerciseValidator.ValidateAsync(exerciseDto);
            if (!validationResult.IsValid)
            {
                throw new Exceptions.ValidationException(validationResult.Errors);
            }

            try
            {
                var existingExercise = await _supabaseClient.From<Exercise>().Filter("id", Postgrest.Constants.Operator.Equals, id).Single();
                if (existingExercise == null)
                {
                    throw new NotFoundException($"Exercise with id {id} not found.");
                }

                _mapper.Map(exerciseDto, existingExercise);

                await _supabaseClient.From<Exercise>().Update(existingExercise);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to update exercise with id {id}.", id);
                if (ex.Message.Contains("duplicate key value violates unique constraint"))
                {
                    throw new DuplicateEntryException("An exercise with this name already exists.", ex);
                }
                throw new DatabaseException($"An error occurred while updating the exercise with id {id}.", ex);
            }
        }

        public async Task<IEnumerable<ExerciseHistoryDto>> GetExerciseHistoryAsync(long exerciseId)
        {
            try
            {
                var response = await _supabaseClient.From<ExerciseSet>()
                    .Select("*, session_exercises!inner(*, sessions!inner(*))")
                    .Where(es => es.SessionExercise.ExerciseId == exerciseId)
                    .Get();

                var history = response.Models.Select(set => new ExerciseHistoryDto
                {
                    SessionDate = set.SessionExercise.Session.StartTime,
                    Weight = set.Weight,
                    Reps = set.Reps,
                    IsFailure = set.IsFailure
                }).OrderBy(h => h.SessionDate);

                return history;
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to retrieve history for exercise {exerciseId}.", exerciseId);
                throw new DatabaseException("An error occurred while retrieving the exercise history.", ex);
            }
        }
    }
}

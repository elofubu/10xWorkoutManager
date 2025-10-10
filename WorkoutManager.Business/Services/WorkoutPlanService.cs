using AutoMapper;
using Microsoft.Extensions.Logging;
using WorkoutManager.Business.DTOs;
using WorkoutManager.Business.Exceptions;
using WorkoutManager.Business.Interfaces;
using WorkoutManager.Data.Models;
using FluentValidation;
using Postgrest.Exceptions;

namespace WorkoutManager.Business.Services
{
    public class WorkoutPlanService : IWorkoutPlanService
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly IMapper _mapper;
        private readonly ILogger<WorkoutPlanService> _logger;
        private readonly IValidator<CreateWorkoutPlanDto> _createWorkoutPlanValidator;
        private readonly IValidator<WorkoutPlanDto> _updateWorkoutPlanValidator;

        public WorkoutPlanService(Supabase.Client supabaseClient, IMapper mapper, ILogger<WorkoutPlanService> logger, IValidator<CreateWorkoutPlanDto> createWorkoutPlanValidator, IValidator<WorkoutPlanDto> updateWorkoutPlanValidator)
        {
            _supabaseClient = supabaseClient;
            _mapper = mapper;
            _logger = logger;
            _createWorkoutPlanValidator = createWorkoutPlanValidator;
            _updateWorkoutPlanValidator = updateWorkoutPlanValidator;
        }

        public async Task<WorkoutPlanDto> CreateWorkoutPlanAsync(WorkoutPlanDto workoutPlanDto)
        {
            var validationResult = await _createWorkoutPlanValidator.ValidateAsync(_mapper.Map<CreateWorkoutPlanDto>(workoutPlanDto));

            if (!validationResult.IsValid)
            {
                throw new Exceptions.ValidationException(validationResult.Errors);
            }

            try
            {
                var workoutPlan = _mapper.Map<WorkoutPlan>(workoutPlanDto);
                var response = await _supabaseClient.From<WorkoutPlan>().Insert(workoutPlan);
                var createdWorkoutPlan = response.Models.First();

                return _mapper.Map<WorkoutPlanDto>(createdWorkoutPlan);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to create workout plan.");
                // This is a simplistic mapping. In a real application, you might want to inspect the Postgres error code.
                if (ex.Message.Contains("duplicate key value violates unique constraint"))
                {
                    throw new DuplicateEntryException("A workout plan with this name already exists.", ex);
                }
                throw new DatabaseException("An error occurred while creating the workout plan.", ex);
            }
        }

        public async Task DeleteWorkoutPlanAsync(long id)
        {
            try
            {
                var existingPlan = await _supabaseClient.From<WorkoutPlan>().Filter("id", Postgrest.Constants.Operator.Equals, id).Single();
                if (existingPlan == null)
                {
                    throw new NotFoundException($"Workout plan with id {id} not found.");
                }

                await _supabaseClient.From<WorkoutPlan>().Delete(existingPlan);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to delete workout plan with id {id}.", id);
                throw new DatabaseException($"An error occurred while deleting the workout plan with id {id}.", ex);
            }
        }

        public async Task<IEnumerable<WorkoutPlanDto>> GetAllWorkoutPlansAsync()
        {
            try
            {
                var response = await _supabaseClient.From<WorkoutPlan>().Get();
                var workoutPlans = response.Models;

                return _mapper.Map<IEnumerable<WorkoutPlanDto>>(workoutPlans);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to retrieve workout plans.");
                throw new DatabaseException("An error occurred while retrieving the workout plans.", ex);
            }
        }

        public async Task<IEnumerable<WorkoutPlanSummaryDto>> GetAllWorkoutPlanSummariesAsync()
        {
            try
            {
                var response = await _supabaseClient.From<WorkoutPlan>().Get();
                var workoutPlans = response.Models;

                return _mapper.Map<IEnumerable<WorkoutPlanSummaryDto>>(workoutPlans);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to retrieve workout plan summaries.");
                throw new DatabaseException("An error occurred while retrieving the workout plan summaries.", ex);
            }
        }

        public async Task<WorkoutPlanDto> GetWorkoutPlanByIdAsync(long id)
        {
            try
            {
                var response = await _supabaseClient.From<WorkoutPlan>().Filter("id", Postgrest.Constants.Operator.Equals, id).Single();

                if (response == null)
                {
                    throw new NotFoundException($"Workout plan with id {id} not found.");
                }

                return _mapper.Map<WorkoutPlanDto>(response);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to retrieve workout plan with id {id}.", id);
                throw new DatabaseException($"An error occurred while retrieving the workout plan with id {id}.", ex);
            }
        }

        public async Task UpdateWorkoutPlanAsync(long id, WorkoutPlanDto workoutPlanDto)
        {
            if (id != workoutPlanDto.Id)
            {
                throw new ArgumentException("ID mismatch");
            }

            var validationResult = await _updateWorkoutPlanValidator.ValidateAsync(workoutPlanDto);
            if (!validationResult.IsValid)
            {
                throw new Exceptions.ValidationException(validationResult.Errors);
            }

            try
            {
                var existingPlan = await _supabaseClient.From<WorkoutPlan>().Filter("id", Postgrest.Constants.Operator.Equals, id).Single();
                if (existingPlan == null)
                {
                    throw new NotFoundException($"Workout plan with id {id} not found.");
                }

                _mapper.Map(workoutPlanDto, existingPlan);

                await _supabaseClient.From<WorkoutPlan>().Update(existingPlan);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to update workout plan with id {id}.", id);
                if (ex.Message.Contains("duplicate key value violates unique constraint"))
                {
                    throw new DuplicateEntryException("A workout plan with this name already exists.", ex);
                }
                throw new DatabaseException($"An error occurred while updating the workout plan with id {id}.", ex);
            }
        }
    }
}

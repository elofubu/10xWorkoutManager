using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WorkoutManager.Business.DTOs;
using WorkoutManager.Business.Interfaces;
using WorkoutManager.Business.Exceptions;
using WorkoutManager.Data.Models;
using Postgrest.Exceptions;
using System.Linq;

namespace WorkoutManager.Business.Services
{
    public class TrainingDayService : ITrainingDayService
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly IMapper _mapper;
        private readonly ILogger<TrainingDayService> _logger;

        public TrainingDayService(Supabase.Client supabaseClient, IMapper mapper, ILogger<TrainingDayService> logger)
        {
            _supabaseClient = supabaseClient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<TrainingDayDto> AddTrainingDayAsync(long planId, string name)
        {
            try
            {
                var plan = await _supabaseClient.From<WorkoutPlan>().Filter("id", Postgrest.Constants.Operator.Equals, planId).Single();
                if (plan == null)
                {
                    throw new NotFoundException($"Workout plan with id {planId} not found.");
                }

                var maxOrder = await _supabaseClient.From<TrainingDay>()
                                       .Where(td => td.PlanId == planId)
                                       .Get();

                var newTrainingDay = new TrainingDay
                {
                    PlanId = planId,
                    Name = name,
                    Order = (short)(maxOrder.Models.Count > 0 ? maxOrder.Models.Max(td => td.Order) + 1 : 1)
                };

                var response = await _supabaseClient.From<TrainingDay>().Insert(newTrainingDay);
                var createdTrainingDay = response.Models.First();

                return _mapper.Map<TrainingDayDto>(createdTrainingDay);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to add training day to plan {planId}.", planId);
                throw new DatabaseException("An error occurred while adding the training day.", ex);
            }
        }

        public async Task AddExerciseToTrainingDayAsync(long trainingDayId, long exerciseId)
        {
            try
            {
                var trainingDay = await _supabaseClient.From<TrainingDay>().Filter("id", Postgrest.Constants.Operator.Equals, trainingDayId).Single();
                if (trainingDay == null)
                {
                    throw new NotFoundException($"Training day with id {trainingDayId} not found.");
                }

                var exercise = await _supabaseClient.From<Exercise>().Filter("id", Postgrest.Constants.Operator.Equals, exerciseId).Single();
                if (exercise == null)
                {
                    throw new NotFoundException($"Exercise with id {exerciseId} not found.");
                }

                var maxOrder = await _supabaseClient.From<PlanDayExercise>()
                                       .Where(pde => pde.TrainingDayId == trainingDayId)
                                       .Get();

                var newPlanDayExercise = new PlanDayExercise
                {
                    TrainingDayId = trainingDayId,
                    ExerciseId = exerciseId,
                    Order = (short)(maxOrder.Models.Count > 0 ? maxOrder.Models.Max(pde => pde.Order) + 1 : 1)
                };

                await _supabaseClient.From<PlanDayExercise>().Insert(newPlanDayExercise);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to add exercise {exerciseId} to training day {trainingDayId}.", exerciseId, trainingDayId);
                throw new DatabaseException("An error occurred while adding the exercise to the training day.", ex);
            }
        }

        public async Task RemoveTrainingDayAsync(long trainingDayId)
        {
            try
            {
                var trainingDay = await _supabaseClient.From<TrainingDay>().Filter("id", Postgrest.Constants.Operator.Equals, trainingDayId).Single();
                if (trainingDay == null)
                {
                    throw new NotFoundException($"Training day with id {trainingDayId} not found.");
                }

                await _supabaseClient.From<TrainingDay>().Delete(trainingDay);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to remove training day {trainingDayId}.", trainingDayId);
                throw new DatabaseException("An error occurred while removing the training day.", ex);
            }
        }

        public async Task RemoveExerciseFromTrainingDayAsync(long planDayExerciseId)
        {
            try
            {
                var planDayExercise = await _supabaseClient.From<PlanDayExercise>().Filter("id", Postgrest.Constants.Operator.Equals, planDayExerciseId).Single();
                if (planDayExercise == null)
                {
                    throw new NotFoundException($"Plan day exercise with id {planDayExerciseId} not found.");
                }

                await _supabaseClient.From<PlanDayExercise>().Delete(planDayExercise);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to remove exercise from training day for plan day exercise {planDayExerciseId}.", planDayExerciseId);
                throw new DatabaseException("An error occurred while removing the exercise from the training day.", ex);
            }
        }

        public async Task UpdateTrainingDayAsync(TrainingDayDto trainingDay)
        {
            try
            {
                var existingTrainingDay = await _supabaseClient.From<TrainingDay>().Filter("id", Postgrest.Constants.Operator.Equals, trainingDay.Id).Single();
                if (existingTrainingDay == null)
                {
                    throw new NotFoundException($"Training day with id {trainingDay.Id} not found.");
                }

                _mapper.Map(trainingDay, existingTrainingDay);

                await _supabaseClient.From<TrainingDay>().Update(existingTrainingDay);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to update training day {trainingDayId}.", trainingDay.Id);
                throw new DatabaseException("An error occurred while updating the training day.", ex);
            }
        }

        public async Task<TrainingDayDto> GetTrainingDayByIdAsync(long trainingDayId)
        {
            try
            {
                var response = await _supabaseClient.From<TrainingDay>().Filter("id", Postgrest.Constants.Operator.Equals, trainingDayId).Single();
                if (response == null)
                {
                    throw new NotFoundException($"Training day with id {trainingDayId} not found.");
                }
                return _mapper.Map<TrainingDayDto>(response);
            }
            catch (PostgrestException ex)
            {
                _logger.LogError(ex, "Failed to retrieve training day with id {trainingDayId}.", trainingDayId);
                throw new DatabaseException($"An error occurred while retrieving the training day with id {trainingDayId}.", ex);
            }
        }
    }
}

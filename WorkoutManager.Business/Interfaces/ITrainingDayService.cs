using WorkoutManager.Business.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkoutManager.Business.Interfaces
{
    public interface ITrainingDayService
    {
        Task<TrainingDayDto> AddTrainingDayAsync(long planId, string name);
        Task RemoveTrainingDayAsync(long trainingDayId);
        Task UpdateTrainingDayAsync(TrainingDayDto trainingDay);
        Task AddExerciseToTrainingDayAsync(long trainingDayId, long exerciseId);
        Task RemoveExerciseFromTrainingDayAsync(long planDayExerciseId);
        Task<TrainingDayDto> GetTrainingDayByIdAsync(long trainingDayId);
    }
}

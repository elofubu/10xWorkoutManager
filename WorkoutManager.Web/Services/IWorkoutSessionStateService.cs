using WorkoutManager.Web.Helpers;

namespace WorkoutManager.Web.Services
{
    public interface IWorkoutSessionStateService
    {
        Task SaveStateAsync(WorkoutSessionState state);
        Task<WorkoutSessionState?> LoadStateAsync();
        Task ClearStateAsync();
    }
}

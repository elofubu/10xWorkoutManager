using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using WorkoutManager.Web.Helpers;

namespace WorkoutManager.Web.Services
{
    public class WorkoutSessionStateService : IWorkoutSessionStateService
    {
        private const string STORAGE_KEY = "workoutSessionState";
        private readonly ProtectedSessionStorage _sessionStorage;

        public WorkoutSessionStateService(ProtectedSessionStorage sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        public async Task SaveStateAsync(WorkoutSessionState state)
        {
            state.Timestamp = DateTime.UtcNow;
            await _sessionStorage.SetAsync(STORAGE_KEY, state);
        }

        public async Task<WorkoutSessionState?> LoadStateAsync()
        {
            try
            {
                var result = await _sessionStorage.GetAsync<WorkoutSessionState>(STORAGE_KEY);
                return result.Success ? result.Value : null;
            }
            catch
            {
                // If deserialization fails (e.g., schema changed), clear stale data
                await ClearStateAsync();
                return null;
            }
        }

        public async Task ClearStateAsync()
        {
            await _sessionStorage.DeleteAsync(STORAGE_KEY);
        }
    }
}

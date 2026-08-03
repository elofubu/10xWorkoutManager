using Blazored.SessionStorage;
using WorkoutManager.Web.Helpers;

namespace WorkoutManager.Web.Services
{
    public class WorkoutSessionStateService : IWorkoutSessionStateService
    {
        private const string STORAGE_KEY = "workoutSessionState";
        private readonly ISessionStorageService _sessionStorage;

        public WorkoutSessionStateService(ISessionStorageService sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        public async Task SaveStateAsync(WorkoutSessionState state)
        {
            state.Timestamp = DateTime.UtcNow;
            await _sessionStorage.SetItemAsync(STORAGE_KEY, state);
        }

        public async Task<WorkoutSessionState?> LoadStateAsync()
        {
            try
            {
                return await _sessionStorage.GetItemAsync<WorkoutSessionState>(STORAGE_KEY);
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
            await _sessionStorage.RemoveItemAsync(STORAGE_KEY);
        }
    }
}

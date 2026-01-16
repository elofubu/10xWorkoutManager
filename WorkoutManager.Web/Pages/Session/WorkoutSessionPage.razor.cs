using Microsoft.AspNetCore.Components;
using WorkoutManager.BusinessLogic.DTOs;
using MudBlazor;
using WorkoutManager.Web.Services;
using WorkoutManager.Web.Helpers;

namespace WorkoutManager.Web.Pages.Session
{
    public partial class WorkoutSessionPage
    {
        [Parameter]
        public long SessionId { get; set; }

        [Inject]
        private ISessionService SessionService { get; set; } = default!;

        [Inject]
        private IExerciseService ExerciseService { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private IWorkoutSessionStateService StateService { get; set; } = default!;

        private SessionDetailsDto? _session;
        private SessionExerciseDetailsDto? _currentExercise;

        private int? _reps;
        private decimal? _weight;

        private Dictionary<long, PreviousExercisePerformanceDto> _previousSessionData = new();
        private Dictionary<long, string> _exerciseNames = new();
        private string? _sessionNotes;
        private int _index;
        private bool _isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            _session = await SessionService.GetSessionDetailsAsync(SessionId);

            if (_session is null)
            {
                // TODO: Handle case where session is not found, maybe navigate away
                return;
            }

            _sessionNotes = _session.Notes;

            foreach (var exercise in _session.Exercises)
            {
                var previous = await ExerciseService.GetPreviousSessionExerciseAsync(exercise.ExerciseId, _session.TrainingDayId.Value);
                if (previous is not null)
                {
                    _previousSessionData[exercise.ExerciseId] = previous;
                }
                var exerciseDetails = await ExerciseService.GetExerciseByIdAsync(exercise.ExerciseId);
                if (exerciseDetails is not null)
                {
                    _exerciseNames[exercise.ExerciseId] = exerciseDetails.Name;
                }
            }

            _currentExercise = _session.Exercises.OrderBy(e => e.Order).FirstOrDefault();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                try
                {
                    // Restore state from session storage after first render
                    await RestoreStateFromLocalStorageAsync();
                }
                finally
                {
                    _isLoading = false;
                    StateHasChanged();
                }
            }
        }

        private string GetExerciseName(long exerciseId) => _exerciseNames.GetValueOrDefault(exerciseId, $"Exercise {exerciseId}");

        private async Task AddSet()
        {
            if (!_reps.HasValue) return;

            _currentExercise.Sets.Add(new ExerciseSetDto { Reps = _reps.Value, Weight = _weight.HasValue ? _weight.Value : 0 });

            // Save state to localStorage after adding a set
            await SaveStateToLocalStorageAsync();
        }

        private async Task RemoveSet(long setIndex)
        {
            if (!_currentExercise.Sets.Any(s => s.Id == setIndex)) return;

            var setToRemove = _currentExercise.Sets.First(s => s.Id == setIndex);
            _currentExercise.Sets.Remove(setToRemove);

            // Save state to localStorage after removing a set
            await SaveStateToLocalStorageAsync();
        }

        private async Task NextStep()
        {
            _isLoading = true;
            try
            {
                var payload = new UpdateSessionExerciseDto
                {
                    Notes = _currentExercise.Notes,
                    Skipped = _currentExercise.Skipped,
                    Sets = _currentExercise.Skipped ? new List<ExerciseSetDto>() : _currentExercise.Sets.Where(s => s.Reps != 0)?.ToList() ?? new List<ExerciseSetDto>()
                };

                await SessionService.UpdateSessionExerciseAsync(_session.Id, _currentExercise.Id, payload);

                // Clear localStorage after successfully saving to database
                await StateService.ClearStateAsync();
            }
            finally
            {
                _index++;
                _isLoading = false;
                _currentExercise = _session!.Exercises.OrderBy(e => e.Order).ElementAt(_index);
                _weight = 0;
                _reps = 0;
            }
        }

        private async Task CompleteSession()
        {
            _isLoading = true;

            try
            {
                var payload = new UpdateSessionExerciseDto
                {
                    Notes = _currentExercise.Notes,
                    Skipped = _currentExercise.Skipped,
                    Sets = _currentExercise.Skipped ? new List<ExerciseSetDto>() : _currentExercise.Sets.Where(s => s.Reps != 0)?.ToList() ?? new List<ExerciseSetDto>()
                };

                await SessionService.UpdateSessionExerciseAsync(_session.Id, _currentExercise.Id, payload);

                await SessionService.FinishSessionAsync(_session.Id, _sessionNotes);

                // Clear localStorage after successfully completing the session
                await StateService.ClearStateAsync();

                NavigationManager.NavigateTo("/history");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task CancelWorkout()
        {
            _isLoading = true;
            try
            {
                await SessionService.FinishSessionAsync(_session.Id, _session.Notes);

                // Clear localStorage when cancelling workout
                await StateService.ClearStateAsync();

                NavigationManager.NavigateTo("/");
            }
            catch
            {
                _isLoading = false;
            }
        }

        private async Task RestoreStateFromLocalStorageAsync()
        {
            var savedState = await StateService.LoadStateAsync();

            // No saved state or session mismatch - nothing to restore
            if (savedState == null || savedState.SessionId != SessionId)
            {
                if (savedState != null)
                {
                    // Clear stale data from different session
                    await StateService.ClearStateAsync();
                }
                return;
            }

            // Find the exercise that was being worked on
            var savedExercise = _session?.Exercises?.FirstOrDefault(e => e.Id == savedState.CurrentExerciseId);

            // Exercise not found in current session (shouldn't happen, but safety check)
            if (savedExercise == null)
            {
                await StateService.ClearStateAsync();
                return;
            }

            // Check if this exercise already has saved sets in the backend
            // If it does, it means the data was saved elsewhere - ignore localStorage
            if (savedExercise.Sets.Any())
            {
                // Data already saved to backend, localStorage is stale
                await StateService.ClearStateAsync();
                return;
            }

            // Exercise has no saved sets - restore in-progress data from localStorage
            _index = savedState.CurrentIndex;
            _currentExercise = _session.Exercises.OrderBy(e => e.Order).ElementAt(_index);
            _currentExercise.Sets = savedState.UnsavedSets;
            _reps = savedState.InputReps;
            _weight = savedState.InputWeight;
        }

        private async Task SaveStateToLocalStorageAsync()
        {
            if (_session == null || _currentExercise == null)
                return;

            var state = new WorkoutSessionState
            {
                SessionId = SessionId,
                CurrentExerciseId = _currentExercise.Id,
                CurrentIndex = _index,
                UnsavedSets = _currentExercise.Sets.ToList(),
                InputReps = _reps,
                InputWeight = _weight
            };

            await StateService.SaveStateAsync(state);
        }
    }
}

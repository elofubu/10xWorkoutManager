using Microsoft.AspNetCore.Components;
using WorkoutManager.BusinessLogic.DTOs;
using MudBlazor;
using WorkoutManager.Web.Services;
using WorkoutManager.Web.Components;
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

        [Inject]
        private IDialogService DialogService { get; set; } = default!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = default!;

        private SessionDetailsDto? _session;
        private SessionExerciseDetailsDto? _currentExercise;

        private int? _reps;
        private decimal? _weight;

        // Exercise pinned next to the current one (superset) with its own weight/reps inputs.
        private SessionExerciseDetailsDto? _pinnedExercise;
        private int? _pinnedReps;
        private decimal? _pinnedWeight;

        private Dictionary<long, PreviousExercisePerformanceDto> _previousSessionData = new();
        private Dictionary<long, string> _exerciseNames = new();
        private string? _sessionNotes;
        private int _index;
        private bool _isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            if (_session is not null) return;

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

        private List<SessionExerciseDetailsDto> OrderedExercises => _session!.Exercises.OrderBy(e => e.Order).ToList();

        // Any other exercise of this workout can be pinned next to the current one (not only the next one).
        private IEnumerable<SessionExerciseDetailsDto> PinnableExercises =>
            OrderedExercises.Where(e => e.Id != _currentExercise?.Id);

        private async Task AddSetAsync(SessionExerciseDetailsDto exercise, decimal? weight, int? reps)
        {
            // A set needs at least one rep; 0 kg is fine (bodyweight exercises).
            if (reps is not > 0) return;

            exercise.Sets.Add(new ExerciseSetDto { Reps = reps.Value, Weight = weight ?? 0 });

            // Save state to storage after adding a set
            await SaveStateToLocalStorageAsync();
        }

        private async Task RemoveSetAsync(SessionExerciseDetailsDto exercise, ExerciseSetDto set)
        {
            // Remove the exact set that was clicked (new, unsaved sets all have Id 0).
            exercise.Sets.Remove(set);

            // Save state to storage after removing a set
            await SaveStateToLocalStorageAsync();
        }

        private async Task SetNotesAsync(SessionExerciseDetailsDto exercise, string? notes)
        {
            // Saved with the exercise on Next / Complete / Unpin; kept in storage until then.
            exercise.Notes = notes;
            await SaveStateToLocalStorageAsync();
        }

        private async Task PinExerciseAsync(SessionExerciseDetailsDto exercise)
        {
            _pinnedExercise = exercise;
            _pinnedWeight = 0;
            _pinnedReps = 0;

            await SaveStateToLocalStorageAsync();
        }

        private async Task UnpinExerciseAsync()
        {
            if (_pinnedExercise is null) return;

            // Save what was logged on the pinned exercise before it leaves the screen.
            _isLoading = true;
            try
            {
                await PersistExerciseAsync(_pinnedExercise);
            }
            catch
            {
                Snackbar.Add("Failed to save the pinned exercise", Severity.Error);
                _isLoading = false;
                return;
            }

            _pinnedExercise = null;
            _isLoading = false;

            await SaveStateToLocalStorageAsync();
        }

        private async Task PersistExerciseAsync(SessionExerciseDetailsDto exercise)
        {
            var payload = new UpdateSessionExerciseDto
            {
                Notes = exercise.Notes,
                Skipped = exercise.Skipped,
                Sets = exercise.Skipped ? new List<ExerciseSetDto>() : exercise.Sets.Where(s => s.Reps != 0).ToList()
            };

            await SessionService.UpdateSessionExerciseAsync(_session!.Id, exercise.Id, payload);
        }

        // Moving to the pinned exercise itself ends the pin: it is the current exercise now.
        private void ReleasePinIfCurrent()
        {
            if (_pinnedExercise is not null && _pinnedExercise.Id == _currentExercise?.Id)
            {
                _pinnedExercise = null;
            }
        }

        private async Task NextStep()
        {
            _isLoading = true;
            try
            {
                await PersistExerciseAsync(_currentExercise!);

                // A pinned exercise stays on screen, so its sets are saved too.
                if (_pinnedExercise is not null)
                {
                    await PersistExerciseAsync(_pinnedExercise);
                }

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
                ReleasePinIfCurrent();
            }

            // Keep the pin across a page refresh.
            if (_pinnedExercise is not null)
            {
                await SaveStateToLocalStorageAsync();
            }
        }

        private async Task PreviousStep()
        {
            if (_index <= 0) return;

            _index--;
            _currentExercise = _session!.Exercises.OrderBy(e => e.Order).ElementAt(_index);
            _weight = 0;
            _reps = 0;
            ReleasePinIfCurrent();

            if (_pinnedExercise is not null)
            {
                await SaveStateToLocalStorageAsync();
            }
        }

        private async Task CompleteSession()
        {
            _isLoading = true;

            try
            {
                await PersistExerciseAsync(_currentExercise!);

                if (_pinnedExercise is not null)
                {
                    await PersistExerciseAsync(_pinnedExercise);
                }

                await SessionService.FinishSessionAsync(_session!.Id, _sessionNotes);

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
            var parameters = new DialogParameters
            {
                { nameof(ConfirmationDialog.Title), "Discard Workout" },
                { nameof(ConfirmationDialog.ContentText), "Discard this workout? Everything you logged in it will be deleted and it won't appear in your history." },
                { nameof(ConfirmationDialog.ConfirmButtonText), "Discard" }
            };

            var dialog = await DialogService.ShowAsync<ConfirmationDialog>("Discard Workout", parameters);
            var result = await dialog.Result;

            if (result is null || result.Canceled)
            {
                return;
            }

            _isLoading = true;
            try
            {
                // Delete the session (its exercises and sets are removed with it) instead of
                // finishing it, so a cancelled workout never shows up in the history.
                await SessionService.DeleteSessionAsync(_session!.Id);

                // Clear localStorage when cancelling workout
                await StateService.ClearStateAsync();

                NavigationManager.NavigateTo("/");
            }
            catch
            {
                Snackbar.Add("Failed to discard the workout", Severity.Error);
                _isLoading = false;
            }
        }

        private async Task RestoreStateFromLocalStorageAsync()
        {
            // Session must be loaded first
            if (_session?.Exercises == null)
            {
                return;
            }

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
            var savedExercise = _session.Exercises.FirstOrDefault(e => e.Id == savedState.CurrentExerciseId);

            // Exercise not found in current session (shouldn't happen, but safety check)
            if (savedExercise == null)
            {
                await StateService.ClearStateAsync();
                return;
            }

            // Check if this exercise already has saved sets in the backend
            // If it does, it means the data was saved elsewhere - ignore localStorage
            if (savedExercise.Sets?.Any() == true)
            {
                // Data already saved to backend, localStorage is stale — except for a pinned exercise,
                // which keeps unsaved sets of its own.
                RestorePinnedExercise(savedState);
                if (_pinnedExercise is null)
                {
                    await StateService.ClearStateAsync();
                }

                return;
            }

            // Exercise has no saved sets - restore in-progress data from localStorage
            _index = savedState.CurrentIndex;
            var exercises = _session.Exercises.OrderBy(e => e.Order).ToList();
            if (_index >= 0 && _index < exercises.Count)
            {
                _currentExercise = exercises[_index];
                _currentExercise.Sets = savedState.UnsavedSets ?? new List<ExerciseSetDto>();
                _currentExercise.Notes = savedState.UnsavedNotes ?? _currentExercise.Notes;
                _reps = savedState.InputReps;
                _weight = savedState.InputWeight;
            }

            RestorePinnedExercise(savedState);
        }

        private void RestorePinnedExercise(WorkoutSessionState savedState)
        {
            if (savedState.PinnedExerciseId is not long pinnedId || _session?.Exercises == null)
            {
                return;
            }

            var pinned = _session.Exercises.FirstOrDefault(e => e.Id == pinnedId);

            // Unknown exercise, or the same one that is current now: nothing to pin.
            if (pinned == null || pinned.Id == _currentExercise?.Id)
            {
                return;
            }

            if (savedState.PinnedSets != null)
            {
                pinned.Sets = savedState.PinnedSets;
            }

            pinned.Notes = savedState.PinnedNotes ?? pinned.Notes;

            _pinnedExercise = pinned;
            _pinnedReps = savedState.PinnedInputReps;
            _pinnedWeight = savedState.PinnedInputWeight;
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
                InputWeight = _weight,
                UnsavedNotes = _currentExercise.Notes,
                PinnedExerciseId = _pinnedExercise?.Id,
                PinnedSets = _pinnedExercise?.Sets.ToList(),
                PinnedNotes = _pinnedExercise?.Notes,
                PinnedInputReps = _pinnedReps,
                PinnedInputWeight = _pinnedWeight
            };

            await StateService.SaveStateAsync(state);
        }
    }
}

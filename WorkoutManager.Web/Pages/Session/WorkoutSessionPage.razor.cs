using Microsoft.AspNetCore.Components;
using WorkoutManager.BusinessLogic.DTOs;
using MudBlazor;
using WorkoutManager.Web.Services;

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

        private SessionDetailsDto? _session;
        private SessionExerciseDetailsDto? _currentExercise;

        private Dictionary<long, PreviousExercisePerformanceDto> _previousSessionData = new();
        private Dictionary<long, string> _exerciseNames = new();
        private string? _sessionNotes;
        private int _index;
        private bool _isLoading = true;
        private bool _isSavingExercise = false;
        private bool _isCanceling = false;

        protected override async Task OnInitializedAsync()
        {
            try
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
                    //Add 3 empty sets objects
                    while (exercise.Sets.Count < 3)
                    {
                        exercise.Sets.Add(new ExerciseSetDto());
                    }

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
            finally
            {
                _isLoading = false;
            }
        }

        private string GetExerciseName(long exerciseId) => _exerciseNames.GetValueOrDefault(exerciseId, $"Exercise {exerciseId}");

        private void AddSet(SessionExerciseDetailsDto exercise)
        {
            exercise.Sets.Add(new ExerciseSetDto());
        }

        private void RemoveSet(SessionExerciseDetailsDto exercise, int setIndex)
        {
            if (setIndex >= 0 && setIndex < exercise.Sets.Count)
            {
                exercise.Sets.RemoveAt(setIndex);
            }
        }

        private async Task NextStep(int index)
        {
            if(index <= 0) return;

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
            }
            finally
            {
                _index = index;
                _isLoading = false;
                _currentExercise = _session!.Exercises.OrderBy(e => e.Order).ElementAt(_index);
            }
        }

        private async Task CompleteSession()
        {
            _isSavingExercise = true;

            try
            {
                await SessionService.FinishSessionAsync(_session.Id, _sessionNotes);
                NavigationManager.NavigateTo("/history");
            }
            finally
            {
                _isSavingExercise = false;
            }
        }

        private async Task CancelWorkout()
        {
            _isCanceling = true;
            try
            {
                await SessionService.FinishSessionAsync(_session.Id, _session.Notes);
                NavigationManager.NavigateTo("/");
            }
            catch
            {
                _isCanceling = false;
            }
        }
    }
}

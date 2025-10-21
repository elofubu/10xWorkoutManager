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
        private MudStepper _stepper = new()!;
        private Dictionary<long, PreviousExercisePerformanceDto> _previousSessionData = new();
        private Dictionary<long, string> _exerciseNames = new();
        private string? _sessionNotes;
        private int _index;

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
                //Add 3 empty sets objects
                while (exercise.Sets.Count < 3)
                {
                    exercise.Sets.Add(new ExerciseSetDto());
                }

                var previous = await ExerciseService.GetPreviousSessionExerciseAsync(exercise.ExerciseId);
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

        private async Task NextStep()
        {
            var currentExercise = _session!.Exercises.OrderBy(e => e.Order).ElementAt(_index);

            var payload = new UpdateSessionExerciseDto
            {
                Notes = currentExercise.Notes,
                Skipped = currentExercise.Skipped,
                Sets = currentExercise.Skipped ? new List<ExerciseSetDto>() : currentExercise.Sets.Where(s => s.Weight != 0 && s.Reps != 0)?.ToList() ?? new List<ExerciseSetDto>()
            };

            await SessionService.UpdateSessionExerciseAsync(_session.Id, currentExercise.Id, payload);

            if (_index == _session.Exercises.Count - 1)
            {
                await SessionService.FinishSessionAsync(_session.Id, _sessionNotes);
                NavigationManager.NavigateTo("/history");
            }
            else
            {
                await _stepper.NextStepAsync();
            }
        }

        private async Task CancelWorkout()
        {
            await SessionService.FinishSessionAsync(_session.Id, _session.Notes);
            NavigationManager.NavigateTo("/");
        }

        //private async Task PreviousStep()
        //{
        //    await _stepper.PreviousStepAsync();
        //}
    }
}

using Microsoft.AspNetCore.Components;
using WorkoutManager.BusinessLogic.DTOs;
using MudBlazor;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Pages.Session
{
    public partial class WorkoutSessionPage
    {
        [Parameter]
        public int TrainingDayId { get; set; }

        [Inject]
        private ISessionService SessionService { get; set; } = default!;

        [Inject]
        private IExerciseService ExerciseService { get; set; } = default!;
        
        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

    private SessionDetailsDto? _session;
    private MudStepper _stepper = new()!;
    private Dictionary<int, PreviousExercisePerformanceDto> _previousSessionData = new();
    private Dictionary<int, string> _exerciseNames = new();
    private string? _sessionNotes;

    protected override async Task OnInitializedAsync()
    {
        _session = await SessionService.StartSessionAsync(TrainingDayId);
        foreach (var exercise in _session.Exercises)
        {
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

        private string GetExerciseName(int exerciseId) => _exerciseNames.GetValueOrDefault(exerciseId, $"Exercise {exerciseId}");

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
        var activeStepIndex = _stepper.ActiveIndex;
        var currentExercise = _session!.Exercises.OrderBy(e => e.Order).ElementAt(activeStepIndex);
        
        var payload = new UpdateSessionExerciseDto
        {
            Notes = currentExercise.Notes,
            Skipped = currentExercise.Skipped,
            Sets = currentExercise.Skipped ? new List<ExerciseSetDto>() : currentExercise.Sets
        };
        await SessionService.UpdateSessionExerciseAsync(_session.Id, currentExercise.Id, payload);

        if (activeStepIndex == _session.Exercises.Count - 1)
        {
            await SessionService.FinishSessionAsync(_session.Id, _sessionNotes);
            NavigationManager.NavigateTo("/history");
        }
        else
        {
            await _stepper.NextStepAsync();
        }
    }

        private async Task PreviousStep()
        {
            await _stepper.PreviousStepAsync();
        }
    }
}

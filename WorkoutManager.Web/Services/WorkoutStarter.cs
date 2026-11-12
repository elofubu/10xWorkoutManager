using Microsoft.AspNetCore.Components;
using MudBlazor;
using WorkoutManager.Web.Components;

namespace WorkoutManager.Web.Services;

/// <summary>
/// Starts a workout for a training day: resumes or replaces an unfinished session (after asking),
/// otherwise creates a new one, and navigates to the workout page. Shared by every screen that
/// has a "Start" button.
/// </summary>
public interface IWorkoutStarter
{
    Task StartWorkoutAsync(long trainingDayId);
}

public class WorkoutStarter : IWorkoutStarter
{
    private readonly ISessionService _sessionService;
    private readonly IDialogService _dialogService;
    private readonly NavigationManager _navigationManager;
    private readonly ISnackbar _snackbar;

    public WorkoutStarter(
        ISessionService sessionService,
        IDialogService dialogService,
        NavigationManager navigationManager,
        ISnackbar snackbar)
    {
        _sessionService = sessionService;
        _dialogService = dialogService;
        _navigationManager = navigationManager;
        _snackbar = snackbar;
    }

    public async Task StartWorkoutAsync(long trainingDayId)
    {
        try
        {
            var activeSession = await _sessionService.GetActiveSessionAsync();
            if (activeSession != null)
            {
                var dialog = await _dialogService.ShowAsync<ActiveSessionDialog>("Active Session Found");
                var result = await dialog.Result;

                if (result is not null && !result.Canceled && result.Data is string choice)
                {
                    if (choice == "continue")
                    {
                        _navigationManager.NavigateTo($"/session/workout/{activeSession.Id}");
                    }
                    else if (choice == "finish_and_start_new")
                    {
                        await _sessionService.FinishSessionAsync(activeSession.Id, activeSession.Notes);
                        var newSession = await _sessionService.StartSessionAsync(trainingDayId);
                        _navigationManager.NavigateTo($"/session/workout/{newSession.Id}");
                    }
                }
            }
            else
            {
                var newSession = await _sessionService.StartSessionAsync(trainingDayId);
                _navigationManager.NavigateTo($"/session/workout/{newSession.Id}");
            }
        }
        catch (Exception ex)
        {
            _snackbar.Add($"Error starting workout: {ex.Message}", Severity.Error);
        }
    }
}

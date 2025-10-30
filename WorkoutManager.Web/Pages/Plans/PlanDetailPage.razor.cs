using Microsoft.AspNetCore.Components;
using MudBlazor;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.Web.Components;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Pages.Plans;

public partial class PlanDetailPage
{
    [Parameter]
    public long Id { get; set; }

    [Inject]
    private IWorkoutPlanService WorkoutPlanService { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private ISessionService SessionService { get; set; } = default!;

    private WorkoutPlanDetailDto? _plan;
    private bool _isEditMode;
    private bool _isEditingName;
    private string _editedPlanName = string.Empty;
    private bool _isLoading = true;
    private bool _isSavingName = false;
    private bool _isDeleting = false;
    private bool _isAddingExercise = false;
    private bool _isDeletingExercise = false;
    private bool _isStartingWorkout = false;
    private long? _deletingExerciseId = null;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        try
        {
            await LoadPlan();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task LoadPlan()
    {
        _plan = await WorkoutPlanService.GetWorkoutPlanByIdAsync(Id);
        _editedPlanName = _plan?.Name ?? string.Empty;
    }

    private void StartEditingName()
    {
        _isEditingName = true;
    }

    private async Task SavePlanName()
    {
        if (_plan == null) return;

        _isSavingName = true;
        try
        {
            await WorkoutPlanService.UpdateWorkoutPlanAsync(Id, new UpdateWorkoutPlanDto(
                _editedPlanName,
                _plan.TrainingDays.Select(td => new UpdateTrainingDayOrderCommand((int)td.Id, td.Order)).ToList()
            ));
            _isEditingName = false;
            await LoadPlan();
            Snackbar.Add("Plan name updated successfully", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
        finally
        {
            _isSavingName = false;
        }
    }

    private void CancelEditName()
    {
        _editedPlanName = _plan?.Name ?? string.Empty;
        _isEditingName = false;
    }

    private async Task DeletePlan()
    {
        var parameters = new DialogParameters
        {
            { nameof(ConfirmationDialog.Title), "Delete Plan" },
            { nameof(ConfirmationDialog.ContentText), "Are you sure you want to delete this plan? This action cannot be undone." },
            { nameof(ConfirmationDialog.ConfirmButtonText), "Delete" }
        };

        var dialog = await DialogService.ShowAsync<ConfirmationDialog>("Delete Plan", parameters);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            _isDeleting = true;
            try
            {
                await WorkoutPlanService.DeleteWorkoutPlanAsync(Id);
                Snackbar.Add("Plan deleted successfully", Severity.Success);
                NavigationManager.NavigateTo("/");
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
                _isDeleting = false;
            }
        }
    }

    private async Task AddExercise(long trainingDayId)
    {
        var dialogOptions = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseButton = true,
        };

        var dialog = await DialogService.ShowAsync<ExercisePickerDialog>("Add Exercise", dialogOptions);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled && result.Data is ExerciseDto selectedExercise)
        {
            _isAddingExercise = true;
            try
            {
                var trainingDay = _plan!.TrainingDays.First(td => td.Id == trainingDayId);
                var nextOrder = trainingDay.Exercises.Any() ? trainingDay.Exercises.Max(e => e.Order) + 1 : 1;

                await WorkoutPlanService.AddExerciseToTrainingDayAsync(Id, trainingDayId, new AddExerciseToTrainingDayCommand
                {
                    ExerciseId = (int)selectedExercise.Id,
                    Order = nextOrder
                });
                await LoadPlan();
                Snackbar.Add("Exercise added successfully", Severity.Success);
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
            finally
            {
                _isAddingExercise = false;
            }
        }
    }

    private async Task StartWorkout(long trainingDayId)
    {
        _isStartingWorkout = true;
        try
        {
            var activeSession = await SessionService.GetActiveSessionAsync();
            if (activeSession != null)
            {
                var dialog = await DialogService.ShowAsync<ActiveSessionDialog>("Active Session Found");
                var result = await dialog.Result;

                if (result is not null && !result.Canceled && result.Data is string choice)
                {
                    if (choice == "continue")
                    {
                        NavigationManager.NavigateTo($"/session/workout/{activeSession.Id}");
                    }
                    else if (choice == "finish_and_start_new")
                    {
                        await SessionService.FinishSessionAsync(activeSession.Id, activeSession.Notes);
                        var newSession = await SessionService.StartSessionAsync(trainingDayId);
                        NavigationManager.NavigateTo($"/session/workout/{newSession.Id}");
                    }
                }
                else
                {
                    _isStartingWorkout = false;
                }
            }
            else
            {
                var newSession = await SessionService.StartSessionAsync(trainingDayId);
                NavigationManager.NavigateTo($"/session/workout/{newSession.Id}");
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error starting workout: {ex.Message}", Severity.Error);
            _isStartingWorkout = false;
        }
    }

    private async Task DeleteExercise(long trainingDayId, long planDayExerciseId)
    {
        var parameters = new DialogParameters
        {
            { nameof(ConfirmationDialog.Title), "Remove Exercise" },
            { nameof(ConfirmationDialog.ContentText), "Are you sure you want to remove this exercise?" },
            { nameof(ConfirmationDialog.ConfirmButtonText), "Remove" }
        };

        var dialog = await DialogService.ShowAsync<ConfirmationDialog>("Remove Exercise", parameters);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            _isDeletingExercise = true;
            _deletingExerciseId = planDayExerciseId;
            try
            {
                await WorkoutPlanService.RemoveExerciseFromTrainingDayAsync(Id, trainingDayId, planDayExerciseId);
                await LoadPlan();
                Snackbar.Add("Exercise removed successfully", Severity.Success);
            }
            catch
            {
                Snackbar.Add("Failed to remove exercise", Severity.Error);
                _isDeletingExercise = false;
                _deletingExerciseId = null;
            }
        }
    }
}

using Microsoft.AspNetCore.Components;
using MudBlazor;
using WorkoutManager.Web.Components;
using WorkoutManager.Web.Services;
using Blazored.LocalStorage;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.Web.Pages;

public partial class WelcomePage
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private IWorkoutPlanService WorkoutPlanService { get; set; } = default!;

    [Inject]
    private ILocalStorageService LocalStorage { get; set; } = default!;

    private async Task CreateFirstPlan()
    {
        var dialog = await DialogService.ShowAsync<CreatePlanDialog>("Create Your First Plan");
        var result = await dialog.Result;

        if (result is not null && !result.Canceled && result.Data is CreateWorkoutPlanDto newPlan)
        {
            await WorkoutPlanService.CreateWorkoutPlanAsync(newPlan);
            await LocalStorage.SetItemAsync("hasSeenWelcome", true);
            NavigationManager.NavigateTo("/");
        }
    }

    private async Task SkipWelcome()
    {
        await LocalStorage.SetItemAsync("hasSeenWelcome", true);
        NavigationManager.NavigateTo("/");
    }
}


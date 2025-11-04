using Microsoft.AspNetCore.Components;
using MudBlazor;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.Web.Components;
using WorkoutManager.Web.Services;
using Blazored.LocalStorage;

namespace WorkoutManager.Web.Pages;

public partial class Home
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IWorkoutPlanService WorkoutPlanService { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private IAuthService AuthService { get; set; } = default!;

    [Inject]
    private ILocalStorageService LocalStorage { get; set; } = default!;

    private IEnumerable<WorkoutPlanDto>? _plans = null;
    private PaginationInfo _pagination = new();
    private bool _isLoading = true;
    private bool _isCreatingPlan = false;

    private int PageCount => _pagination.PageSize > 0 ? (int)Math.Ceiling((double)_pagination.TotalCount / _pagination.PageSize) : 0;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        try
        {
            var hasSeenWelcome = await LocalStorage.GetItemAsync<bool>("hasSeenWelcome");
            var isAuthenticated = await AuthService.IsAuthenticatedAsync();

            if (isAuthenticated && !hasSeenWelcome)
            {
                NavigationManager.NavigateTo("/welcome");
                return;
            }

            var result = await WorkoutPlanService.GetWorkoutPlansAsync();
            _plans = result.Data;
            _pagination = result.Pagination;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void NavigateToPlan(long planId)
    {
        NavigationManager.NavigateTo($"/plans/{planId}");
    }

    private async Task CreateNewPlan()
    {
        var dialogOptions = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true,
            CloseButton = true,
        };

        var dialog = await DialogService.ShowAsync<CreatePlanDialog>("Create New Plan", dialogOptions);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled && result.Data is CreateWorkoutPlanDto newPlan)
        {
            _isCreatingPlan = true;
            try
            {
                await WorkoutPlanService.CreateWorkoutPlanAsync(newPlan);
                var paginatedResult = await WorkoutPlanService.GetWorkoutPlansAsync();
                _plans = paginatedResult.Data;
                _pagination = paginatedResult.Pagination;
                StateHasChanged();
            }
            finally
            {
                _isCreatingPlan = false;
            }
        }
    }
}

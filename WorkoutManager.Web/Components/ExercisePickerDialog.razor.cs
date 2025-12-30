using Microsoft.AspNetCore.Components;
using MudBlazor;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Components
{
    public partial class ExercisePickerDialog
    {
        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = default!;

        [Inject]
        private IExerciseService ExerciseService { get; set; } = default!;

        [Inject]
        private IMuscleGroupService MuscleGroupService { get; set; } = default!;

        [Inject]
        private IDialogService DialogService { get; set; } = default!;

        private string? _search;
        private long? _selectedMuscleGroupId;
        private IEnumerable<ExerciseDto> _exercises = new List<ExerciseDto>();
        private List<MuscleGroupDto> _muscleGroups = new();
        private PaginationInfo _pagination = new();
        private bool _isLoading = true;
        private bool _isSearching = false;

        private int PageCount => _pagination.PageSize > 0 ? (int)Math.Ceiling((double)_pagination.TotalCount / _pagination.PageSize) : 0;

        protected override async Task OnInitializedAsync()
        {
            _isLoading = true;
            try
            {
                var muscleGroupsResult = await MuscleGroupService.GetMuscleGroupsAsync();
                _muscleGroups = muscleGroupsResult.ToList();
                await LoadExercises();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task LoadExercises(int page = 1)
        {
            _isSearching = true;
            try
            {
                var result = await ExerciseService.GetExercisesAsync(_search, _selectedMuscleGroupId, page);
                _exercises = result.Data;
                _pagination = result.Pagination;
                StateHasChanged();
            }
            finally
            {
                _isSearching = false;
            }
        }

        private async Task SearchExercises()
        {
            await LoadExercises();
        }

        private async Task ClearFilter()
        {
            _selectedMuscleGroupId = null;
            await LoadExercises();
        }

        private async Task PageChanged(int page)
        {
            await LoadExercises(page);
        }

        private void SelectExercise(ExerciseDto exercise)
        {
            MudDialog.Close(DialogResult.Ok(exercise));
        }

        private async Task OpenCreateExerciseDialog()
        {
            var dialogOptions = new DialogOptions
            {
                FullWidth = true,
                NoHeader = true,
                FullScreen = true
            };

            var dialog = await DialogService.ShowAsync<CreateExerciseDialog>("Create Exercise", dialogOptions);
            var result = await dialog.Result;

            if (result is not null && !result.Canceled)
            {
                await LoadExercises();
            }
        }

        private void Cancel() => MudDialog.Cancel();
    }
}

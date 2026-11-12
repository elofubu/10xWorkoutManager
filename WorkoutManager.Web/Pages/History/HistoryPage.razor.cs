using Microsoft.AspNetCore.Components;
using MudBlazor;
using WorkoutManager.Web.Components;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Pages.History
{
    public partial class HistoryPage
    {
        [Inject]
        private ISessionService SessionService { get; set; } = default!;

        [Inject]
        private IDialogService DialogService { get; set; } = default!;

        [Inject]
        private ISnackbar Snackbar { get; set; } = default!;

        private long? _deletingSessionId;

        private IEnumerable<SessionSummaryDto> _sessions = new List<SessionSummaryDto>();
        private PaginationInfo _pagination = new();
        private bool _isLoading = true;
        private bool _isPaginationLoading = false;
        private int _currentPage = 1;

        private int PageCount => _pagination.PageSize > 0 ? (int)Math.Ceiling((double)_pagination.TotalCount / _pagination.PageSize) : 0;

        protected override async Task OnInitializedAsync()
        {
            _isLoading = true;
            try
            {
                var result = await SessionService.GetSessionHistoryAsync();
                _sessions = result.Data;
                _pagination = result.Pagination;
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task PageChanged(int page)
        {
            _isPaginationLoading = true;
            _currentPage = page;
            try
            {
                //no paggination implemented yet
                //var result = await SessionService.GetSessionHistoryAsync(page);
                var result = await SessionService.GetSessionHistoryAsync();
                _sessions = result.Data;
                _pagination = result.Pagination;
                StateHasChanged();
            }
            finally
            {
                _isPaginationLoading = false;
            }
        }

        private async Task DeleteSession(long sessionId)
        {
            var parameters = new DialogParameters
            {
                { nameof(ConfirmationDialog.Title), "Delete Workout" },
                { nameof(ConfirmationDialog.ContentText), "Delete this workout from your history? This cannot be undone." },
                { nameof(ConfirmationDialog.ConfirmButtonText), "Delete" }
            };

            var dialog = await DialogService.ShowAsync<ConfirmationDialog>("Delete Workout", parameters);
            var result = await dialog.Result;

            if (result is null || result.Canceled)
            {
                return;
            }

            _deletingSessionId = sessionId;
            try
            {
                await SessionService.DeleteSessionAsync(sessionId);
                _sessions = _sessions.Where(s => s.Id != sessionId).ToList();
                Snackbar.Add("Workout deleted", Severity.Success);
            }
            catch
            {
                Snackbar.Add("Failed to delete the workout", Severity.Error);
            }
            finally
            {
                _deletingSessionId = null;
            }
        }

        // private void NavigateToSession(long sessionId)
        // {
        //     NavigationManager.NavigateTo($"/history/{sessionId}");
        // }

        private string HandleDecimalInWeight(decimal weight)
        {
            if ((weight % 1) == 0)
            {
                if (weight % 10 == 0)
                {
                    return Math.Round(weight).ToString("F0");
                }

                return Math.Round(weight).ToString();
            }

            return weight.ToString();
        }
    }
}
